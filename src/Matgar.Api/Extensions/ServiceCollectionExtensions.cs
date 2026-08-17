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


            return services;

        }
    }
}
