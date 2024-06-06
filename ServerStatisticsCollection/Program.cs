using RabbitMQ.Client;
using ServerStatisticsCollection.Configurations;
using ServerStatisticsCollection.Interfaces;
using ServerStatisticsCollection.Messaging;
using ServerStatisticsCollection.Services;

namespace ServerStatisticsCollection;

class Program
{
    static async Task Main(string[] args)
    {
        var currentNamespace = typeof(Program).Namespace?.Split('.')[0];

        var configManager = new ConfigurationManager(currentNamespace);

        if (int.TryParse(configManager.GetData("ServerStatisticsConfig", "SamplingIntervalSeconds"),
                out var samplingIntervalSeconds))
        {
            var factory = new ConnectionFactory() { HostName = "localhost" };

            IMessageQueuePublisher messageQueuePublisher = new RabbitMqMessageQueuePublisher(factory);

            var collector = new StatisticsCollector(samplingIntervalSeconds, messageQueuePublisher, configManager);

            using (var cancellationTokenSource = new CancellationTokenSource())
            {
                Console.CancelKeyPress += (sender, eventArgs) =>
                {
                    eventArgs.Cancel = true;
                    cancellationTokenSource.Cancel();
                };

                await collector.StartCollectingAsync(cancellationTokenSource.Token, "ServerStatistics");
            }
        }
    }
}