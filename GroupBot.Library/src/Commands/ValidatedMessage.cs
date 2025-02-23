using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ValidatedMessage
{
    public required Chat Chat { get; init; }
    public required User? From { get; init; }
    public required int MessageId { get; init; }
    public required ValidatedMessage? ReplyToMessage { get; init; }
    private ValidatedMessage() { }

    public static ValidatedMessage? FromTelegramMessage(Message? message)
    {
        if (message?.Chat == null)
        {
            return null;
        }

        return new ValidatedMessage
        {
            Chat = message.Chat,
            From = message.From,
            MessageId = message.MessageId,
            ReplyToMessage = FromTelegramMessage(message.ReplyToMessage)
        };
    }
}
