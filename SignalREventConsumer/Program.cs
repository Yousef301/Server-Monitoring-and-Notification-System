using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.Extensions.Configuration;

namespace SignalREventConsumer
{
    class Program
    {
        static async Task Main(string[] args)
        {
            IConfigurationRoot configuration = new ConfigurationBuilder()
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables()
                .Build();
            
            var signalRUrl = configuration["SignalRConfig:SignalRUrl"];
            var serilogPath = configuration["SerilogLogger:Path"];
            

            if (signalRUrl == null)
            {
                Console.WriteLine("SignalR URL is not configured.");
                return;
            }

            var connection = new HubConnectionBuilder()
                .WithUrl(signalRUrl)
                .Build();

            connection.On<string, string>("ReceiveMessage", (user, message) => 
            { 
                Console.WriteLine($"{user}: {message}"); 
            });

            try
            {
                await connection.StartAsync();
                Console.WriteLine("Connection started.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error starting connection: {ex.Message}");
                return;
            }

            var resetEvent = new ManualResetEvent(false);
            resetEvent.WaitOne();
        }
    }
}