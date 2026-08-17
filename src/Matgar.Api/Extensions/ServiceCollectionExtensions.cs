namespace Matgar.Api.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddApiServices(this IServiceCollection services)
        {

            services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();


            return services;

        }
    }
}
