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
        var chatId = "-1002396986878";
        var text = "Hello world!";

        Message message = await Bot.SendMessage(chatId, text);

        Assert.Equal(text, message.Text);
        Assert.Equal(MessageType.Text, message.Type);
        Assert.Equal(chatId, message.Chat.Id.ToString());
        Assert.NotEqual(default, message.Date);
        Assert.NotNull(message.From);
    }
}