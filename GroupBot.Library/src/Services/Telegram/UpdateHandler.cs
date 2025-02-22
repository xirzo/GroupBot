using GroupBot.Library.Commands.Parser;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Services.Telegram;

public class UpdateHandler : IUpdateHandler
{
    private readonly CommandParser _parser;

    public UpdateHandler(CommandParser parser)
    {
        _parser = parser;
    }

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message || update.Message is null || string.IsNullOrEmpty(update.Message.Text) || update.Message.Text[0] != '/')
        {
            return Task.CompletedTask;
        }

        CommandParseResult parseResult = _parser.Parse(update.Message.Text);

        if (!parseResult.Success)
        {
            Console.WriteLine(parseResult.ErrorMessage);

            botClient.SendMessage(
                 chatId: update.Message.Chat.Id,
                 text: parseResult.ErrorMessage,
                 replyParameters: new ReplyParameters { MessageId = update.Message.MessageId });

            return Task.CompletedTask;
        }

        return parseResult.Command.Execute(update.Message, botClient, parseResult.Parameters);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}
