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
using ConfigurationManager = ServerStatisticsCollection.Configurations.ConfigurationManager;

namespace MessageProcessingAndAnomalyDetection;

class Program
{
    static async Task Main(string[] args)
    {
        var currentNamespace = typeof(Program).Namespace?.Split('.')[0];

        var configManager = new ConfigurationManager(currentNamespace);

        var anomalyDetectionConfigurations = GetAnomalyDetectionConfigurations(configManager);
        var connectionString = configManager.GetSection("MongoDBConnectionStrings");
        var signalRHubUrl = configManager.GetSection("SignalRConfig");
        var serilogConfigs = configManager.GetSection("SerilogLogger");


        if (connectionString != null && signalRHubUrl != null && anomalyDetectionConfigurations != null &&
            serilogConfigs != null)
        {
            var mongoDb = GetMongoDb(connectionString["ConnectionString"], connectionString["DatabaseName"]);
            var serverStatisticsMongoDbRepository = new ServerStatisticsMongoDbRepository(mongoDb);

            if (signalRHubUrl["SignalRUrl"] != null && serilogConfigs["Path"] != null)
            {
                var hubConnection = CreateHubConnection(signalRHubUrl["SignalRUrl"]);
                var anomalyDetectionService = new AnomalyDetectionService();

                Log.Logger = LoggerFactory.CreateLogger(serilogConfigs["Path"]);

                var sendAlertsService =
                    new SendAlertsService(anomalyDetectionService, anomalyDetectionConfigurations, hubConnection);

                await hubConnection.StartAsync();

                var factory = new ConnectionFactory { HostName = "localhost" };
                var messageQueue =
                    new RabbitMqMessageQueueConsumer(factory, serverStatisticsMongoDbRepository, sendAlertsService);
                messageQueue.StartConsuming<ServerStatistics>("ServerStatistics", "ServerStatistics");
            }
            else
            {
                ConsoleOutput.MessageDisplay(
                    "SignalRUrl or logging path is not configured in the appsettings.json file.");
            }
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

    private static AnomalyDetectionConfigurations? GetAnomalyDetectionConfigurations(ConfigurationManager configManager)
    {
        var anomalyDetectionConfigurations = configManager.GetSection("AnomalyDetectionConfig");

        if (anomalyDetectionConfigurations == null)
        {
            return null;
        }

        return new AnomalyDetectionConfigurations()
        {
            MemoryUsageAnomalyThresholdPercentage = GetValueOrDefault(anomalyDetectionConfigurations,
                "MemoryUsageAnomalyThresholdPercentage"),
            CpuUsageAnomalyThresholdPercentage =
                GetValueOrDefault(anomalyDetectionConfigurations, "CpuUsageAnomalyThresholdPercentage"),
            MemoryUsageThresholdPercentage =
                GetValueOrDefault(anomalyDetectionConfigurations, "MemoryUsageThresholdPercentage"),
            CpuUsageThresholdPercentage =
                GetValueOrDefault(anomalyDetectionConfigurations, "CpuUsageThresholdPercentage"),
        };
    }

    private static double GetValueOrDefault(IConfigurationSection configuration, string key)
    {
        var value = configuration[key];
        return value != null ? Math.Round(double.Parse(value)) : 0;
    }

    private static HubConnection CreateHubConnection(string url)
    {
        return new HubConnectionBuilder().WithUrl(url).Build();
    }
}