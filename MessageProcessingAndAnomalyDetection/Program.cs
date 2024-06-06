using MessageProcessingAndAnomalyDetection.Messaging;
using MessageProcessingAndAnomalyDetection.Models;
using MessageProcessingAndAnomalyDetection.Repositories;
using MongoDB.Driver;
using RabbitMQ.Client;
using ServerStatisticsCollection.Configurations;

namespace MessageProcessingAndAnomalyDetection;

class Program
{
    static async Task Main(string[] args)
    {
        var currentNamespace = typeof(Program).Namespace?.Split('.')[0];

        var configManager = new ConfigurationManager(currentNamespace);

        var connectionString = configManager.GetSection("MongoDBConnectionStrings");


        if (connectionString != null)
        {
            var mongoDb = GetMongoDb(connectionString["ConnectionString"], connectionString["DatabaseName"]);
            var serverStatisticsMongoDbRepository = new ServerStatisticsMongoDbRepository(mongoDb);

            var factory = new ConnectionFactory() { HostName = "localhost" };
            var messageQueue =
                new RabbitMqMessageQueueConsumer(factory, serverStatisticsMongoDbRepository);
            messageQueue.StartConsuming<ServerStatistics>("ServerStatistics", "ServerStatistics");
        }
    }

    private static IMongoDatabase GetMongoDb(string? connectionString, string? databaseName)
    {
        var client = new MongoClient(connectionString);
        return client.GetDatabase(databaseName);
    }
}