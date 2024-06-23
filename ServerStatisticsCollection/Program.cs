using Microsoft.Extensions.Configuration;
using RabbitMQ.Client;
using ServerStatisticsCollection.Interfaces;
using ServerStatisticsCollection.Messaging;
using ServerStatisticsCollection.Services;

namespace ServerStatisticsCollection;

class Program
{
    static async Task Main(string[] args)
    {
        IConfigurationRoot configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();

        var samplingIntervalSecondsString = configuration["ServerStatisticsConfig:SamplingIntervalSeconds"];

        if (int.TryParse(samplingIntervalSecondsString,
                out var samplingIntervalSeconds))
        {
            var rabbitHost = configuration["RabbitMQConfig:Host"];
            var factory = new ConnectionFactory() { HostName = rabbitHost };

            IMessageQueuePublisher messageQueuePublisher = new RabbitMqMessageQueuePublisher(factory);

            var collector = new StatisticsCollector(samplingIntervalSeconds, messageQueuePublisher, configuration);

            using var cancellationTokenSource = new CancellationTokenSource();
            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancellationTokenSource.Cancel();
            };

            await collector.StartCollectingAsync(cancellationTokenSource.Token, "ServerStatistics");
        }
    }
}