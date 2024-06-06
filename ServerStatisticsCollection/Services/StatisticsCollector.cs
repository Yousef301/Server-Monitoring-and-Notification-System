﻿using System.Diagnostics;
using ServerStatisticsCollection.Configurations;
using ServerStatisticsCollection.Interfaces;
using ServerStatisticsCollection.Models;
using System.Diagnostics;

namespace ServerStatisticsCollection.Services;

public class StatisticsCollector : IStatisticsCollector
{
    private readonly int _samplingIntervalSeconds;
    private readonly PerformanceCounter _cpuCounter;
    private readonly PerformanceCounter _availableMemoryCounter;
    private readonly ConfigurationManager _configManager;

    public StatisticsCollector(int samplingIntervalSeconds, ConfigurationManager configManager)
    {
        _samplingIntervalSeconds = samplingIntervalSeconds;
        _cpuCounter = new PerformanceCounter("Processor", "% Processor Time", "_Total");
        _availableMemoryCounter = new PerformanceCounter("Memory", "Available Bytes");
        _configManager = configManager;
    }

    public async Task StartCollectingAsync(CancellationToken cancellationToken, string exchange)
    {
        var topic = "ServerStatistics." + _configManager.GetData("ServerStatisticsConfig", "ServerIdentifier");

        while (!cancellationToken.IsCancellationRequested)
        {
            var stats = CollectStatistics();
            await Task.Delay(_samplingIntervalSeconds * 1000, cancellationToken);
        }
    }

    private ServerStatistics CollectStatistics()
    {
        double memoryUsage = Process.GetCurrentProcess().WorkingSet64 / (1024 * 1024);
        double availableMemory = Convert.ToDouble(_availableMemoryCounter.NextValue()) / (1024 * 1024);
        double cpuUsage = _cpuCounter.NextValue();
        DateTime timestamp = DateTime.UtcNow;

        return new ServerStatistics(memoryUsage, availableMemory, cpuUsage, timestamp);
    }
}