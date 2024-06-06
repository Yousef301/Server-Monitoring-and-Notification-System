using ServerStatisticsCollection.Interfaces;

namespace ServerStatisticsCollection.Models;

public class ServerStatistics : IServerStatistics
{
    public double MemoryUsage { get; set; }
    public double AvailableMemory { get; set; }
    public double CpuUsage { get; set; }
    public DateTime Timestamp { get; set; }

    public ServerStatistics(double memoryUsage, double availableMemory, double cpuUsage, DateTime timestamp)
    {
        MemoryUsage = memoryUsage;
        AvailableMemory = availableMemory;
        CpuUsage = cpuUsage;
        Timestamp = timestamp;
    }

    public override string ToString()
    {
        return
            $"Memory Usage: {MemoryUsage} MB, Available Memory: {AvailableMemory} MB, CPU Usage: {CpuUsage}%, Timestamp: {Timestamp}";
    }
}