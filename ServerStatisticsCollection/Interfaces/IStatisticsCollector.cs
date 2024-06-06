namespace ServerStatisticsCollection.Interfaces;

public interface IStatisticsCollector
{
    Task StartCollectingAsync(CancellationToken cancellationToken, string exchange);
}