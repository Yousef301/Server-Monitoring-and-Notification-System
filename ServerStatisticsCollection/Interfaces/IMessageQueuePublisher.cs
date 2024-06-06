namespace ServerStatisticsCollection.Interfaces;

public interface IMessageQueuePublisher
{
    void Publish<T>(string exchange, string topic, T message);
}