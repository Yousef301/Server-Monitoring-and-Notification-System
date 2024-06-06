namespace MessageProcessingAndAnomalyDetection.Interfaces.Services;

public interface IAnomalyDetectionService
{
    public Dictionary<string, List<string>>? ServerAnomalyDetection(
        IAnomalyDetectionConfigurations anomalyDetectionConfigurations,
        IServerStatistics previousServerStatistics,
        IServerStatistics currentServerStatistics);
}