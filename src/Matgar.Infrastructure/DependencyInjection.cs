using Hangfire;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Abstractions.Services;
using Matgar.Application.Common.Caching;
using Matgar.Infrastructure.Identity.Entities;
using Matgar.Infrastructure.Identity.Services;
using Matgar.Infrastructure.Options;
using Matgar.Infrastructure.Otions;
using Matgar.Infrastructure.Persistence.Contexts;
using Matgar.Infrastructure.Persistence.Interceptor;
using Matgar.Infrastructure.Persistence.Repositories;
using Matgar.Infrastructure.Persistence.Seeders;
using Matgar.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Matgar.Infrastructure
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPersistence(configuration).
            AddIdentity(configuration).
            AddInfrastructureServices(configuration).
            AddHangfire(configuration).
            AddJwtAuthentication(configuration);


            services.AddScoped<OutboxProcessorJob>();
            services.AddScoped(
            typeof(IGenericRepository<>),
            typeof(GenericRepository<>));
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped<IAppMailService, AppMailService>();
            services.AddScoped<IBackgroundJobService, BackgroundJobService>();

            services.AddScoped<IDataSeeder, RoleSeeder>();
            services.AddScoped<IDataSeeder, AdminSeeder>();
            services.AddScoped<DataSeederRunner>();


            return services;
        }

        private static IServiceCollection AddHangfire(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddHangfire(config =>
            {
                config.UseSqlServerStorage(configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddHangfireServer();
            return services;
        }

        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<AuditSaveChangesInterceptor>();


            services.AddDbContext<ApplicationDbContext>((serviceProvider, options) =>
            {
                options.UseSqlServer(configuration.GetConnectionString("DefaultConnection"));

                options.AddInterceptors(serviceProvider.GetRequiredService<AuditSaveChangesInterceptor>());
            });


            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = configuration.GetConnectionString("Redis");
            });


            services.AddScoped<ICacheService, CacheService>();
            return services;
        }

        private static IServiceCollection AddIdentity(this IServiceCollection services, IConfiguration configuration)
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

        private static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<EmailOptions>(configuration.GetSection("EmailOptions"));
            services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IIdentityService, IdentityService>();
            services.AddScoped<IAccessTokenService, AccessTokenService>();
            services.AddScoped<IRefreshTokenService, RefreshTokenService>();
            services.AddScoped<ICurrentUserService, CurrentUserService>();
            services.AddScoped<ICategoryRepository, CategoryRepository>();
            services.AddSingleton<IDbConnectionFactory, DapperConnectionFactory>();

            return services;
        }
        private static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSettings = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>();


            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            }).
            AddJwtBearer(options =>
            {
                options.SaveToken = true;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,

                    ValidIssuer = jwtSettings.Issuer,
                    ValidAudience = jwtSettings.Audience,
                    IssuerSigningKey = new SymmetricSecurityKey(System.Text.Encoding.UTF8.GetBytes(jwtSettings.Key)),

                    ClockSkew = TimeSpan.Zero

                };

            });


            return services;
        }
        public static async Task SeedDatabaseAsync(
           this IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();

            var runner = scope.ServiceProvider
                .GetRequiredService<DataSeederRunner>();

            await runner.RunAsync();
        }
    }
}

