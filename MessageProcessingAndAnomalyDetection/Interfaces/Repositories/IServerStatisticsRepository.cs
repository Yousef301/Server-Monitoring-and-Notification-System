using MessageProcessingAndAnomalyDetection.Models;

namespace MessageProcessingAndAnomalyDetection.Interfaces.Repositories;

public interface IServerStatisticsRepository
{
    Task AddServerStatisticsAsync(ServerStatistics serverStatistics);

    Task<ServerStatistics> GetLatestServerStatisticsAsync();
}