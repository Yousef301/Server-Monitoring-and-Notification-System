using MessageProcessingAndAnomalyDetection.Interfaces;

namespace MessageProcessingAndAnomalyDetection.Models;

public class AnomalyDetectionConfigurations : IAnomalyDetectionConfigurations
{
    public double MemoryUsageAnomalyThresholdPercentage { get; set; }
    public double CpuUsageAnomalyThresholdPercentage { get; set; }
    public double MemoryUsageThresholdPercentage { get; set; }
    public double CpuUsageThresholdPercentage { get; set; }
}