using System.Text;
using Newtonsoft.Json;
using RabbitMQ.Client;
using ServerStatisticsCollection.Interfaces;

namespace ServerStatisticsCollection.Messaging;

public class RabbitMqMessageQueuePublisher : IMessageQueuePublisher
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqMessageQueuePublisher(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public void Publish<T>(string exchange, string topic, T message)
    {
        using var connection = _connectionFactory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: exchange, type: ExchangeType.Topic);

        var jsonMessage = JsonConvert.SerializeObject(message);
        var body = Encoding.UTF8.GetBytes(jsonMessage);

        channel.BasicPublish(exchange: exchange,
            routingKey: topic,
            basicProperties: null,
            body: body);

        Console.WriteLine($"Published message to topic '{topic}' in RabbitMQ: {jsonMessage}");
    }
}