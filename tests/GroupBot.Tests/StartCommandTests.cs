using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Xunit;

namespace GroupBot.Tests;

public class StartCommandTests
{
    [Fact]
    public async Task Should_Send_Text_Message()
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var botToken = config.GetSection("BotConfiguration")["BotToken"];

        if (botToken == null)
            throw new ArgumentException("Bot token environment variable is missing");

        var botClient = new TelegramBotClient(botToken);
        var chatId = "-1002396986878";
        var text = "Hello world!";

        Message message = await botClient.SendMessage(chatId, text);

        Assert.Equal(text, message.Text);
        Assert.Equal(MessageType.Text, message.Type);
        Assert.Equal(chatId, message.Chat.Id.ToString());
        Assert.NotEqual(default, message.Date);
        Assert.NotNull(message.From);
    }
}