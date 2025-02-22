using GroupBot.Library.Commands.Parser;
using GroupBot.Library.Logging;
using Telegram.Bot;

namespace GroupBot.Library.Services.Telegram;

public class TelegramService : ITelegramService
{
    private readonly TelegramBotClient _botClient;
    private readonly CancellationTokenSource _cts;
    private readonly UpdateHandler _updateHandler;
    private readonly ILogger _logger;

    public TelegramService(TelegramBotClient botClient, CommandParser parser, ILogger logger)
    {
        _botClient = botClient;
        _cts = new CancellationTokenSource();
        _logger = logger;
        _updateHandler = new UpdateHandler(parser, _logger);
    }

    public async Task StartBot()
    {
        var me = await _botClient.GetMe(_cts.Token);

        _botClient.StartReceiving(_updateHandler, cancellationToken: _cts.Token);

        _logger.Info($"@{me.Username} is running...");

        await Task.Delay(-1, _cts.Token);
    }

}
