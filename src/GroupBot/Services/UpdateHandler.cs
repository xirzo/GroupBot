using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using GroupBot.Commands.Abstract;

namespace GroupBot.Services;

public class UpdateHandler : IUpdateHandler
{
    private readonly CommandFactory _factory;

    public UpdateHandler(CommandFactory factory)
    {
        _factory = factory;
    }

    public async Task HandleUpdateAsync(TelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Type != UpdateType.Message)
            return;

        var message = update.Message;
        if (message?.Text == null)
            return;

        var commandKey = message.Text.Split(' ')[0];
        var command = _factory.GetCommand(commandKey);

        if (command != null)
            await command.Execute(message, botClient);
    }

    public Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, HandleErrorSource source,
        CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }

    public Task HandleErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}