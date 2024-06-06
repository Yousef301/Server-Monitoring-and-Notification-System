using Microsoft.AspNetCore.SignalR.Client;
using ServerStatisticsCollection.Configurations;

namespace SignalREventConsumer;

class Program
{
    static async Task Main(string[] args)
    {
        var currentNamespace = typeof(Program).Namespace?.Split('.')[0];

        var configManager = new ConfigurationManager(currentNamespace);
        var signalRUrl = configManager.GetSection("SignalRConfig")["SignalRUrl"];

        if (signalRUrl == null)
        {
            return;
        }

        var connection = new HubConnectionBuilder()
            .WithUrl(signalRUrl)
            .Build();

        await connection.StartAsync();

        connection.On("ReceiveMessage",
            (string user, string message) => { Console.WriteLine($"{user}: {message}"); });

        Console.ReadKey();
    }
}