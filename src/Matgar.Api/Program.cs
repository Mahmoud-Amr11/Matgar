using Matgar.Api.Extensions;
using Matgar.Application;
using Matgar.Infrastructure;
using Serilog;
using Serilog.Events;

namespace Matgar.Api
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
               .WriteTo.Console()
               .CreateLogger();

            try
            {

                var builder = WebApplication.CreateBuilder(args);

                builder.AddSerilogLogging();
                builder.Services.AddApiServices().AddInfrastructure(builder.Configuration)
                    .AddApplication();

                var app = builder.Build();


                if (app.Environment.IsDevelopment())
                {
                    app.MapSwagger();
                    app.MapSwaggerUI();
                }

                await app.UseApiPipeline();

                app.Run();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Application start-up failed");
            }
            finally
            {
                Log.CloseAndFlush();
            }

        }
    }
}
