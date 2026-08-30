using Asp.Versioning;
using Matgar.Api.Middlewares;

namespace Matgar.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
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

            return services;

        }
    }
}
