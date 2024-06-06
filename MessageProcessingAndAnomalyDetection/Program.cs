using MessageProcessingAndAnomalyDetection.Messaging;
using MessageProcessingAndAnomalyDetection.Models;
using RabbitMQ.Client;
using ServerStatisticsCollection.Configurations;

namespace MessageProcessingAndAnomalyDetection;

class Program
{
    static async Task Main(string[] args)
    {
        var factory = new ConnectionFactory() { HostName = "localhost" };
        var messageQueue = new RabbitMqMessageQueueConsumer(factory);
        messageQueue.StartConsuming<ServerStatistics>("ServerStatistics", "ServerStatistics");
    }
}