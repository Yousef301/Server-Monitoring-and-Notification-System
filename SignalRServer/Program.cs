using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;

namespace SignalRServer;

class Program
{
    static void Main(string[] args)
    {
        CreateWebHostBuilder(args).Build().Run();
    }

    private static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
        WebHost.CreateDefaultBuilder(args)
            .UseStartup<Startup>()
            .UseUrls("http://0.0.0.0:5000");
}