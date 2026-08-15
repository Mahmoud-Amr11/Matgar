using Hangfire;
using Matgar.Application.Abstractions.Authentication;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Abstractions.Services;
using Matgar.Infrastructure.Identity.Entities;
using Matgar.Infrastructure.Identity.Services;
using Matgar.Infrastructure.Otions;
using Matgar.Infrastructure.Persistence.Contexts;
using Matgar.Infrastructure.Persistence.Repositories;
using Matgar.Infrastructure.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Matgar.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            AddPersistence(services, configuration);
            AddIdentity(services, configuration);
            AddInfrastructureServices(services, configuration);
            AddHangfire(services, configuration);


            services.AddScoped<OutboxProcessorJob>();
            services.AddScoped(
            typeof(IGenericRepository<>),
            typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAppMailService, AppMailService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            return services;
        }

        private static IServiceCollection AddHangfire(IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddHangfireServer();
            return services;
        }

        private static IServiceCollection AddPersistence(IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));
            });


            return services;
        }

        private static IServiceCollection AddIdentity(IServiceCollection services, IConfiguration configuration)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>(options =>
            {
                options.SignIn.RequireConfirmedAccount = true;
                options.User.RequireUniqueEmail = true;
                options.Lockout.MaxFailedAccessAttempts = 4;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(1);
            })
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();
            return services;
        }

        private static IServiceCollection AddInfrastructureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection("EmailOptions"));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IIdentityService, IdentityService>();

            return services;
        }
    }
}

