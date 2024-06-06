using MessageProcessingAndAnomalyDetection.Models;

namespace MessageProcessingAndAnomalyDetection.Interfaces.Repositories;

public interface IServerStatisticsRepository
{
    void AddServerStatistics(ServerStatistics serverStatistics);

    ServerStatistics GetLatestServerStatistics();
}