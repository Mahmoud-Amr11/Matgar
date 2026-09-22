using Hangfire;
using Matgar.Api.HealthChecks;
using Matgar.Infrastructure;
using Matgar.Infrastructure.Persistence.Outbox;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;

namespace Matgar.Api.Extensions
{
    public static class ApplicationBuilderExtensions
    {
        public static async Task<WebApplication> UseApiPipeline(this WebApplication app)
        {

            ConfigureSwagger(app);
            await app.Services.SeedDatabaseAsync();
            app.UseStatusCodePages();
            app.UseExceptionHandler();
            app.UseStaticFiles();
            //app.UseSerilogRequestLoggingWithDetails();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors(ServiceCollectionExtensions.CorsPolicyName);
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            MapHealthEndpoints(app);
            app.UseHangfireDashboard("/jobs");
            var recurringJobManager =
                  app.Services.GetRequiredService<IRecurringJobManager>();
            recurringJobManager.AddOrUpdate<OutboxProcessorJob>(
                  "process-outbox-messages",
                  job => job.ProcessOutboxMessages(),
                  Cron.Minutely);
            app.MapControllers();
            return app;
        }



        private static void ConfigureSwagger(WebApplication app)
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                options.DisplayRequestDuration();
                options.EnableFilter();
                options.DocExpansion(DocExpansion.None);
                options.InjectJavascript("/swagger/custom.js");
            });
        }



        private static void MapHealthEndpoints(WebApplication app)
        {
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            app.MapHealthChecks("/health/live", new HealthCheckOptions
            {
                Predicate = _ => false,
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate = check => check.Tags.Contains("ready"),
                ResponseWriter = HealthCheckResponseWriter.WriteAsync
            });
        }



    }
}
