using Microsoft.Extensions.Configuration;

namespace TOTP.IntegrationTests.Utils;

public class TestConfiguration
{
    public static IConfiguration GetConfiguration()
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json")
            .AddEnvironmentVariables()
            .Build();
        return configuration;
    }
}