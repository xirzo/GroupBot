using GroupBot.Library.Commands.Abstract;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Services.Telegram;

public class TelegramService : ITelegramService
{
    private readonly TelegramBotClient _botClient;
    private readonly CommandFactory _factory;
    private readonly CancellationTokenSource _cts;
    private readonly UpdateHandler _updateHandler;

    public TelegramService(TelegramBotClient botClient, CommandFactory factory)
    {
        _botClient = botClient;
        _factory = factory;
        _cts = new CancellationTokenSource();
        _updateHandler = new UpdateHandler(factory);

        botClient.OnMessage += HandleMessage;
        botClient.OnUpdate += HandleUpdate;
        botClient.OnError += HandleError;
    }

    public async Task StartBot()
    {
        var me = await _botClient.GetMe(_cts.Token);

        _botClient.StartReceiving(_updateHandler, null, _cts.Token);

        Console.WriteLine($"@{me.Username} is running...");

        await Task.Delay(-1, _cts.Token);
    }
    
    private async Task HandleMessage(Message message, UpdateType type)
    {
        var line = message.Text?.Split(' ')[0].Replace(" ", string.Empty);

        if (line == null)
        {
            return;
        }

        var command = _factory.GetCommand(line);

        if (command == null)
        {
            return;
        }

        await command.Execute(message, _botClient); 
    }

    private async Task HandleUpdate(Update update)
    {
        if (update.Type != UpdateType.Message)
            return;

        var message = update.Message;

        if (message?.Text == null)
        {
            return;
        }

        var line = message.Text.Split(' ')[0];
        var command = _factory.GetCommand(line);

        if (command != null)
        {
            await command.Execute(message, _botClient);
        }
    }

    private Task HandleError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }
}