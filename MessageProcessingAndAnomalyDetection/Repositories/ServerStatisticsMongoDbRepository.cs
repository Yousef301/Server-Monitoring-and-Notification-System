using MessageProcessingAndAnomalyDetection.Interfaces.Repositories;
using MessageProcessingAndAnomalyDetection.Models;
using MongoDB.Driver;

namespace MessageProcessingAndAnomalyDetection.Repositories;

public class ServerStatisticsMongoDbRepository : IServerStatisticsRepository
{
    private readonly IMongoDatabase _db;

    public ServerStatisticsMongoDbRepository(IMongoDatabase db)
    {
        _db = db;
    }

    private IMongoCollection<ServerStatistics> ServerStatistics =>
        _db.GetCollection<ServerStatistics>("ServerMonitoringLogs");

    public void AddServerStatistics(ServerStatistics serverStatistics)
    {
        ServerStatistics.InsertOne(serverStatistics);
    }

    public ServerStatistics GetLatestServerStatistics()
    {
        var projection = Builders<ServerStatistics>.Projection.Exclude("_id");
        return ServerStatistics.Find(_ => true)
            .Project<ServerStatistics>(projection)
            .SortByDescending(x => x.Timestamp)
            .FirstOrDefault();
    }
}