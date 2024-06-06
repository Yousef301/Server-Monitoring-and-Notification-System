using System.Text;
using MessageProcessingAndAnomalyDetection.Interfaces;
using MessageProcessingAndAnomalyDetection.Interfaces.Repositories;
using MessageProcessingAndAnomalyDetection.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageProcessingAndAnomalyDetection.Messaging;

public class RabbitMqMessageQueueConsumer : IMessageQueueConsumer
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly IServerStatisticsRepository _serverStatisticsMongoDbRepository;

    public RabbitMqMessageQueueConsumer(ConnectionFactory connectionFactory,
        IServerStatisticsRepository serverStatisticsMongoDbRepository)
    {
        _connectionFactory = connectionFactory;
        _serverStatisticsMongoDbRepository = serverStatisticsMongoDbRepository;
    }

    public void StartConsuming<T>(string exchange, string topic)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic);

        var queueName = channel.QueueDeclare().QueueName;

        channel.QueueBind(queue: queueName,
            exchange: exchange,
            routingKey: $"{topic}.*");

        var consumer = new EventingBasicConsumer(channel);
        consumer.Received += (model, ea) =>
        {
            var body = ea.Body.ToArray();
            var message = Encoding.UTF8.GetString(body);

            string?[] routingKeyParts = ea.RoutingKey.Split('.');
            var serverIdentifier = routingKeyParts.Length > 1 ? routingKeyParts[1] : null;

            var deserializedMessage = JsonConvert.DeserializeObject<T>(message);

            if (deserializedMessage is ServerStatistics serverStat)
            {
                var previousServerStatistics = _serverStatisticsMongoDbRepository.GetLatestServerStatistics();

                serverStat.ServerIdentifier = serverIdentifier;
                _serverStatisticsMongoDbRepository.AddServerStatistics(serverStat);
                Console.WriteLine(serverStat.ToString());
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);


        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}