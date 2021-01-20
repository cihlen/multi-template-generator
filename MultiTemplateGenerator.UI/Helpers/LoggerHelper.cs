using System.IO;
using Serilog;

namespace MultiTemplateGenerator.UI.Helpers
{
    public static class LoggerHelper
    {
        public static ILogger CreateFileLogger()
        {
            ILogger logger = new LoggerConfiguration()
                .Enrich.FromLogContext()
                .WriteTo.File(Path.Combine(Path.GetTempPath(), "Multi-Template_Generator_Log.txt"), restrictedToMinimumLevel:
                    Serilog.Events.LogEventLevel.Information, rollingInterval: RollingInterval.Day, rollOnFileSizeLimit: true)
                .CreateLogger();

            return logger;
        }
    }
}
