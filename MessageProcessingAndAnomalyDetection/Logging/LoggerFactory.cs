using System.Runtime.CompilerServices;
using Serilog;

namespace MessageProcessingAndAnomalyDetection.Logging;

public static class LoggerFactory
{
    public static ILogger CreateLogger(string path)
    {
        return new LoggerConfiguration()
            .MinimumLevel.Information()
            .WriteTo.File(path)
            .CreateLogger();
    }
}