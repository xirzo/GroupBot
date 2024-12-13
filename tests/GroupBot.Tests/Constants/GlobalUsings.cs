global using Xunit;
global using static GroupBot.Tests.Constants.TestConstants;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;

namespace GroupBot.Tests.Constants;

public static class TestConstants
{
    public static TelegramBotClient Bot { get; }

    static TestConstants()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var botToken = config.GetSection("BotConfiguration")["BotToken"] ?? throw new InvalidOperationException();

        Bot = new TelegramBotClient(botToken);
    }
}