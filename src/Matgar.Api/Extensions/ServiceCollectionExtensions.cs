using Asp.Versioning;
using Matgar.Api.HealthChecks;
using Matgar.Api.Middlewares;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Threading.RateLimiting;

namespace Matgar.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public const string CorsPolicyName = "MatgarCors";

        private static readonly string[] DefaultAllowedOrigins =
        [
            "http://localhost:5173",
            "https://localhost:5173"
        ];

        public static IServiceCollection AddApiServices(
            this IServiceCollection services,
            IConfiguration configuration)
        {

            services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
            services.AddProblemDetails(options =>
            {
                options.CustomizeProblemDetails = context =>
                {
                    context.ProblemDetails.Extensions["traceId"] =
                        context.HttpContext.TraceIdentifier;

                    context.ProblemDetails.Extensions["timestamp"] =
                        DateTime.UtcNow;
                };
            });
            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();
            services.AddExceptionHandler<GlobalExceptionHandler>();
            services.AddApiVersioning(options =>
            {
                options.DefaultApiVersion = new ApiVersion(1, 0);
                options.AssumeDefaultVersionWhenUnspecified = true;
                options.ReportApiVersions = true;
                options.ApiVersionReader = new UrlSegmentApiVersionReader();
            })
                  .AddApiExplorer(options =>
                  {
                      options.GroupNameFormat = "'v'VVV";
                      options.SubstituteApiVersionInUrl = true;
                  });

            services.AddCors(options =>
            {
                var allowedOrigins = configuration
                    .GetSection("Cors:AllowedOrigins")
                    .Get<string[]>();

                if (allowedOrigins is null || allowedOrigins.Length == 0)
                    allowedOrigins = DefaultAllowedOrigins;

                options.AddPolicy(CorsPolicyName, policy =>
                {
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials();
                });
            });

            var rateLimiting = configuration.GetSection("RateLimiting");
            var globalPermitLimit = rateLimiting.GetValue<int?>("GlobalPermitLimit") ?? 100;
            var authPermitLimit = rateLimiting.GetValue<int?>("AuthPermitLimit") ?? 10;
            var rateLimitWindow = TimeSpan.FromSeconds(
                rateLimiting.GetValue<int?>("WindowSeconds") ?? 60);

            services.AddRateLimiter(options =>
            {
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(
                    httpContext => RateLimitPartition.GetFixedWindowLimiter(
                        GetRateLimitPartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = globalPermitLimit,
                            Window = rateLimitWindow,
                            QueueLimit = 0
                        }));

                options.AddPolicy("auth", httpContext =>
                    RateLimitPartition.GetFixedWindowLimiter(
                        GetRateLimitPartitionKey(httpContext),
                        _ => new FixedWindowRateLimiterOptions
                        {
                            PermitLimit = authPermitLimit,
                            Window = rateLimitWindow,
                            QueueLimit = 0
                        }));

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode =
                        StatusCodes.Status429TooManyRequests;

                    if (context.Lease.TryGetMetadata(
                            MetadataName.RetryAfter, out var retryAfter))
                    {
                        context.HttpContext.Response.Headers.RetryAfter =
                            ((int)retryAfter.TotalSeconds).ToString();
                    }

                    await context.HttpContext.Response.WriteAsJsonAsync(
                        new ProblemDetails
                        {
                            Status = StatusCodes.Status429TooManyRequests,
                            Title = "Too many requests",
                            Detail = "Rate limit exceeded. Please try again later."
                        },
                        cancellationToken);
                };
            });

            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>(
                    "database",
                    tags: ["ready", "db"])
                .AddCheck<RedisHealthCheck>(
                    "redis",
                    tags: ["ready", "cache"]);

            return services;

        }

        private static string GetRateLimitPartitionKey(HttpContext httpContext)
        {
            if (httpContext.User.Identity?.IsAuthenticated == true)
            {
                var userId = httpContext.User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                    return $"user:{userId}";
            }

            var ipAddress = httpContext.Connection.RemoteIpAddress?.ToString();

            return string.IsNullOrEmpty(ipAddress) ? "anonymous" : $"ip:{ipAddress}";
        }
    }
}
