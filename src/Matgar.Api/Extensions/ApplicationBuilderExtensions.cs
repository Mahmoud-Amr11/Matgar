using Hangfire;
using Matgar.Infrastructure;
using Matgar.Infrastructure.Services;
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
            //app.UseSerilogRequestLoggingWithDetails();
            app.UseSerilogRequestLogging();
            app.UseHttpsRedirection();
            app.UseRouting();
            //app.UseCors();
            app.UseAuthentication();
            app.UseAuthorization();
            //app.UseRateLimiter();
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
            });
        }



    }
}
