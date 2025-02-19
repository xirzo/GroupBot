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
    }

    public async Task StartBot()
    {
        var me = await _botClient.GetMe(_cts.Token);

        _botClient.StartReceiving(_updateHandler, cancellationToken: _cts.Token);

        Console.WriteLine($"@{me.Username} is running...");

        await Task.Delay(-1, _cts.Token);
    }

}