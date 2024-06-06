using MessageProcessingAndAnomalyDetection.Interfaces;
using MessageProcessingAndAnomalyDetection.Interfaces.Services;

namespace MessageProcessingAndAnomalyDetection.Services;

public class AnomalyDetectionService : IAnomalyDetectionService
{
    public Dictionary<string, List<string>>? ServerAnomalyDetection(
        IAnomalyDetectionConfigurations anomalyDetectionConfigurations,
        IServerStatistics previousServerStatistics, IServerStatistics currentServerStatistics)
    {
        var alerts = new Dictionary<string, List<string>>();

        var memoryUsageIncrease = currentServerStatistics.MemoryUsage > (previousServerStatistics.MemoryUsage *
                                                                         (1 + anomalyDetectionConfigurations
                                                                             .MemoryUsageAnomalyThresholdPercentage));
        var cpuUsageIncrease = currentServerStatistics.CpuUsage > (previousServerStatistics.CpuUsage *
                                                                   (1 + anomalyDetectionConfigurations
                                                                       .CpuUsageAnomalyThresholdPercentage));

        var currentMemoryUsageRatio = currentServerStatistics.MemoryUsage /
                                      (currentServerStatistics.MemoryUsage + currentServerStatistics.AvailableMemory);

        if (memoryUsageIncrease)
        {
            if (!alerts.ContainsKey("Anomaly Alert"))
                alerts["Anomaly Alert"] = new List<string>();
            alerts["Anomaly Alert"].Add("Memory Usage is above threshold");
        }

        if (cpuUsageIncrease)
        {
            if (!alerts.ContainsKey("Anomaly Alert"))
                alerts["Anomaly Alert"] = new List<string>();
            alerts["Anomaly Alert"].Add("Cpu Usage is above threshold");
        }

        if (currentMemoryUsageRatio > anomalyDetectionConfigurations.MemoryUsageThresholdPercentage)
        {
            if (!alerts.ContainsKey("High Alert"))
                alerts["High Alert"] = new List<string>();
            alerts["High Alert"].Add("Memory Usage is high");
        }

        if (currentServerStatistics.CpuUsage > anomalyDetectionConfigurations.CpuUsageThresholdPercentage * 100)
        {
            if (!alerts.ContainsKey("High Alert"))
                alerts["High Alert"] = new List<string>();
            alerts["High Alert"].Add("Cpu Usage is high");
        }

        return alerts.Count > 0 ? alerts : null;
    }
}