
using Hangfire;
using Matgar.Application;
using Matgar.Infrastructure;
using Matgar.Infrastructure.Services;

namespace Matgar.Api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi

            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddInfrastructure(builder.Configuration)
                .AddApplication();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.MapSwagger();
                app.MapSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseAuthorization();
            app.MapHangfireDashboard("/Hangfire");

            var recurringJobManager =
                app.Services.GetRequiredService<IRecurringJobManager>();

            recurringJobManager.AddOrUpdate<OutboxProcessorJob>(
                "process-outbox-messages",
                job => job.ProcessOutboxMessages(),
                Cron.Minutely);
            app.MapControllers();

            app.Run();
        }
    }
}
