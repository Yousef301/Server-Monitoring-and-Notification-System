namespace MessageProcessingAndAnomalyDetection.Interfaces.Services;

public interface ISendAlertsService
{
    public void SendAlerts(IServerStatistics currentServerStatistics, IServerStatistics previousServerStatistics);
}