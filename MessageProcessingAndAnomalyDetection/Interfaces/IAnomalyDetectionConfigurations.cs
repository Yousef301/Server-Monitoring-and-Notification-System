namespace MessageProcessingAndAnomalyDetection.Interfaces;

public interface IAnomalyDetectionConfigurations
{
    public double MemoryUsageAnomalyThresholdPercentage { get; set; }
    public double CpuUsageAnomalyThresholdPercentage { get; set; }
    public double MemoryUsageThresholdPercentage { get; set; }
    public double CpuUsageThresholdPercentage { get; set; }
}