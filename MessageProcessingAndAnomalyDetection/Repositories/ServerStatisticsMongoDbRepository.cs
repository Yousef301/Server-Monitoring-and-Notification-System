using MessageProcessingAndAnomalyDetection.Interfaces.Repositories;
using MessageProcessingAndAnomalyDetection.Models;
using MongoDB.Driver;
using Serilog;

namespace MessageProcessingAndAnomalyDetection.Repositories
{
    public class ServerStatisticsMongoDbRepository : IServerStatisticsRepository
    {
        private readonly IMongoDatabase _db;

        public ServerStatisticsMongoDbRepository(IMongoDatabase db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        private IMongoCollection<ServerStatistics> ServerStatistics =>
            _db.GetCollection<ServerStatistics>("ServerMonitoringLogs");

        public async Task AddServerStatisticsAsync(ServerStatistics serverStatistics)
        {
            try
            {
                await ServerStatistics.InsertOneAsync(serverStatistics);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error adding server statistics.");
            }
        }

        public async Task<ServerStatistics> GetLatestServerStatisticsAsync()
        {
            try
            {
                var projection = Builders<ServerStatistics>.Projection.Exclude("_id");
                var latestStatistics = await ServerStatistics.Find(_ => true)
                    .Project<ServerStatistics>(projection)
                    .SortByDescending(x => x.Timestamp)
                    .FirstOrDefaultAsync();

                return latestStatistics;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error getting latest server statistics.");
                return null;
            }
        }
    }
}