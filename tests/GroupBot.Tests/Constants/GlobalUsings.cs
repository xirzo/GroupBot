global using Xunit;
global using GroupBot.Tests.Constants;
using Microsoft.Extensions.Configuration;

namespace GroupBot.Tests.Constants;

public static class TestConstants
{
    public static readonly string BotToken;

    static TestConstants()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        BotToken = config.GetSection("BotConfiguration")["BotToken"] ?? throw new InvalidOperationException();
    }
}