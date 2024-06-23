using System.Text;
using MessageProcessingAndAnomalyDetection.ConsoleIO.ConsoleInput;
using MessageProcessingAndAnomalyDetection.ConsoleIO.ConsoleOutput;
using MessageProcessingAndAnomalyDetection.Interfaces;
using MessageProcessingAndAnomalyDetection.Interfaces.Repositories;
using MessageProcessingAndAnomalyDetection.Interfaces.Services;
using MessageProcessingAndAnomalyDetection.Models;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Serilog;

namespace MessageProcessingAndAnomalyDetection.Messaging;

public class RabbitMqMessageQueueConsumer : IMessageQueueConsumer
{
    private readonly ConnectionFactory _connectionFactory;
    private readonly IServerStatisticsRepository _serverStatisticsMongoDbRepository;
    private readonly ISendAlertsService _sendAlertsService;

    public RabbitMqMessageQueueConsumer(ConnectionFactory connectionFactory,
        IServerStatisticsRepository serverStatisticsMongoDbRepository,
        ISendAlertsService sendAlertsService)
    {
        _connectionFactory = connectionFactory;
        _serverStatisticsMongoDbRepository = serverStatisticsMongoDbRepository;
        _sendAlertsService = sendAlertsService;
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

            T? deserializedMessage = default;
            try
            {
                deserializedMessage = JsonConvert.DeserializeObject<T>(message);
            }
            catch (JsonException e)
            {
                Log.Error($"Failed to deserialize message: {message}");
            }

            if (deserializedMessage is ServerStatistics serverStat)
            {
                var previousServerStatistics = _serverStatisticsMongoDbRepository.GetLatestServerStatisticsAsync();

                _sendAlertsService.SendAlerts(serverStat, previousServerStatistics.Result);

                serverStat.ServerIdentifier = serverIdentifier;
                _serverStatisticsMongoDbRepository.AddServerStatisticsAsync(serverStat);
                Console.WriteLine(serverStat.ToString());
            }
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);

        ConsoleOutput.MessageDisplay(" Press [enter] to exit.");
        ConsoleUserInput.ReadInput();
    }
}