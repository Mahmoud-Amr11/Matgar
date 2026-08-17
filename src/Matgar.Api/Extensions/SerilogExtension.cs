using Serilog;

namespace Matgar.Api.Extensions
{
    public static class SerilogExtension
    {
        public static WebApplicationBuilder AddSerilogLogging(this WebApplicationBuilder builder)
        {
            builder.Host.UseSerilog((Context, LoggerConfiguration) =>
            {
                LoggerConfiguration.WriteTo.Console();
                LoggerConfiguration.ReadFrom.Configuration(Context.Configuration);

            }
            );
            return builder;
        }
    }
}