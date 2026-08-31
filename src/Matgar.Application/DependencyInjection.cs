using FluentValidation;
using Matgar.Application.Common.Behaviors;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace Matgar.Application
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplication(this IServiceCollection services)
        {
            var appAssembly = typeof(IAssemblyMaker).Assembly;

            AddMediatr(services, appAssembly);
            return services;
        }

        private static IServiceCollection AddMediatr(IServiceCollection services, Assembly assembly)
        {
            services.AddMediatR(cfg =>
            {
                cfg.RegisterServicesFromAssemblies(assembly);

                cfg.AddOpenBehavior(typeof(ValidationBehavior<,>));
                cfg.AddOpenBehavior(typeof(CachingBehavior<,>));
                cfg.AddOpenBehavior(typeof(LoggingBehavior<,>));

            });

            services.AddValidatorsFromAssembly(assembly);
            return services;
        }
    }
}
