using GroupBot.Library.Commands.Abstract;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly CommandFactory _factory;

    public UpdateHandler(CommandFactory factory)
    {
        _factory = factory;
    }

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message)
            return Task.CompletedTask;

        var message = update.Message;
        
        if (message?.Text == null)
            return Task.CompletedTask;

        var commandKey = message.Text.Split(' ')[0];
        var command = _factory.GetCommand(commandKey);

        if (command == null)
        {
            return Task.CompletedTask;
        }
        
        return command.Execute(message, botClient);
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}