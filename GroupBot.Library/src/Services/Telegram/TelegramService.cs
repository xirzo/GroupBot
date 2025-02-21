using Telegram.Bot;

namespace GroupBot.Library.Services.Telegram;

public class TelegramService : ITelegramService
{
    private readonly TelegramBotClient _botClient;
    private readonly CancellationTokenSource _cts;
    private readonly UpdateHandler _updateHandler;

    public TelegramService(TelegramBotClient botClient, UpdateHandler updateHandler)
    {
        _botClient = botClient;
        _cts = new CancellationTokenSource();
        _updateHandler = updateHandler;
    }

    public async Task StartBot()
    {
        var me = await _botClient.GetMe(_cts.Token);

        _botClient.StartReceiving(_updateHandler, cancellationToken: _cts.Token);

        Console.WriteLine($"@{me.Username} is running...");

        await Task.Delay(-1, _cts.Token);
    }

}
