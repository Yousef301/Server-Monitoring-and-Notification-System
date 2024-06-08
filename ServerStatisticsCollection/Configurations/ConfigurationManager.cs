using Microsoft.Extensions.Configuration;

namespace ServerStatisticsCollection.Configurations;

public class ConfigurationManager
{
    private IConfiguration _configuration;

    public ConfigurationManager(string namespaceName)
    {
        var currentDirectory = Directory.GetCurrentDirectory();

        while (!Directory.Exists(Path.Combine(currentDirectory, namespaceName)))
        {
            currentDirectory = Directory.GetParent(currentDirectory)?.FullName;
            if (currentDirectory == null)
            {
                throw new DirectoryNotFoundException("Could not find the project directory.");
            }
        }

        var builder = new ConfigurationBuilder()
            .SetBasePath(currentDirectory)
            .AddJsonFile($"{namespaceName}/appsettings.json", optional: true, reloadOnChange: true);
        _configuration = builder.Build();
    }

    public IConfigurationSection? GetSection(string section)
    {
        var value = _configuration.GetSection(section);
        return value;
    }

    public string? GetData(string section, string key)
    {
        var value = _configuration.GetSection(section)[key];
        return value;
    }

    public IConfiguration GetConfiguration()
    {
        return _configuration;
    }
}