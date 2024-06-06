namespace MessageProcessingAndAnomalyDetection.Interfaces;

public interface IMessageQueueConsumer
{
    void StartConsuming<T>(string exchange, string topic);
}