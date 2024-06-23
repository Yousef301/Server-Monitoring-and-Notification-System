using MessageProcessingAndAnomalyDetection.ConsoleIO.ConsoleOutput;
using MessageProcessingAndAnomalyDetection.Logging;
using MessageProcessingAndAnomalyDetection.Messaging;
using MessageProcessingAndAnomalyDetection.Models;
using MessageProcessingAndAnomalyDetection.Repositories;
using MessageProcessingAndAnomalyDetection.Services;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;
using MongoDB.Driver;
using RabbitMQ.Client;
using Serilog;

namespace MessageProcessingAndAnomalyDetection;

class Program
{
    static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var anomalyDetectionConfigurations = GetAnomalyDetectionConfigurations(configuration);

        var databaseName = configuration["MongoDBConnectionStrings:DatabaseName"];
        var connectionString = configuration["MongoDBConnectionStrings:ConnectionString"];

        var signalRHubUrl = configuration["SignalRConfig:SignalRUrl"];

        var serilogPath = configuration["SerilogLogger:Path"];

        var rabbitHost = configuration["RabbitMqConfig:Host"];


        if (connectionString != null && signalRHubUrl != null && anomalyDetectionConfigurations != null &&
            serilogPath != null && databaseName != null && rabbitHost != null)
        {
            var mongoDb = GetMongoDb(connectionString, databaseName);
            var serverStatisticsMongoDbRepository = new ServerStatisticsMongoDbRepository(mongoDb);

            var hubConnection = CreateHubConnection(signalRHubUrl);
            var anomalyDetectionService = new AnomalyDetectionService();

            Log.Logger = LoggerFactory.CreateLogger(serilogPath);

            var sendAlertsService =
                new SendAlertsService(anomalyDetectionService, anomalyDetectionConfigurations, hubConnection);

            await hubConnection.StartAsync();

            var factory = new ConnectionFactory() { HostName = rabbitHost };

            var messageQueue =
                new RabbitMqMessageQueueConsumer(factory, serverStatisticsMongoDbRepository, sendAlertsService);
            messageQueue.StartConsuming<ServerStatistics>("ServerStatistics", "ServerStatistics");
        }
        else
            ConsoleOutput.MessageDisplay(
                "Check the configuration file 'appsettings.json'.");
    }

    private static IMongoDatabase GetMongoDb(string? connectionString, string? databaseName)
    {
        var client = new MongoClient(connectionString);
        return client.GetDatabase(databaseName);
    }

    private static AnomalyDetectionConfigurations? GetAnomalyDetectionConfigurations(IConfigurationRoot configuration)
    {
        try
        {
            return new AnomalyDetectionConfigurations()
            {
                MemoryUsageAnomalyThresholdPercentage =
                    Double.Parse(configuration["AnomalyDetectionConfig:MemoryUsageAnomalyThresholdPercentage"]),
                CpuUsageAnomalyThresholdPercentage =
                    Double.Parse(configuration["AnomalyDetectionConfig:CpuUsageAnomalyThresholdPercentage"]),
                MemoryUsageThresholdPercentage =
                    Double.Parse(configuration["AnomalyDetectionConfig:MemoryUsageThresholdPercentage"]),
                CpuUsageThresholdPercentage =
                    Double.Parse(configuration["AnomalyDetectionConfig:CpuUsageThresholdPercentage"]),
            };
        }
        catch (Exception e)
        {
            Log.Error(
                "Anomaly detection configurations are not set correctly in the appsettings.json file.");
            return null;
        }
    }

    private static HubConnection CreateHubConnection(string url)
    {
        return new HubConnectionBuilder().WithUrl(url).Build();
    }
}