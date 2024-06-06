using System.Text;
using MessageProcessingAndAnomalyDetection.Interfaces;
using Newtonsoft.Json;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace MessageProcessingAndAnomalyDetection.Messaging;

public class RabbitMqMessageQueueConsumer : IMessageQueueConsumer
{
    private readonly ConnectionFactory _connectionFactory;

    public RabbitMqMessageQueueConsumer(ConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
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

            var deserializedMessage = JsonConvert.DeserializeObject<T>(message);

            Console.WriteLine("Received message: " + deserializedMessage);
        };

        channel.BasicConsume(queue: queueName, autoAck: true, consumer: consumer);


        Console.WriteLine(" Press [enter] to exit.");
        Console.ReadLine();
    }
}