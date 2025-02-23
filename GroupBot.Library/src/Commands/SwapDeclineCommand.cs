using GroupBot.Library.Logging;
using GroupBot.Library.Services.Request;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Commands;

public class SwapDeclineCommand : ICommand
{
    private readonly IRequestService _requestService;
    private readonly ILogger _logger;

    public SwapDeclineCommand(IRequestService requestService, ILogger logger)
    {
        _requestService = requestService;
        _logger = logger;
    }

    public long NumberOfArguments => 0;

    public string GetString() => "Отказаться";
    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        
        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));
        
        var replyParameters = new ReplyParameters
        {
            MessageId = message.MessageId
        };

        if (message.Chat.Type != ChatType.Private)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ответьте на сообщение в личном чате",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            _logger.Info(LogMessages.ErrorOccurred(GetString(), $"{requestingUser} used {GetString()} not in dms", null));
            return;
        }

        if (message.From == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Пользователь не найден", replyParameters: replyParameters);
            _logger.Warn(LogMessages.NotFound(requestingUser, requestingUser));
            return;
        }

        var userId = message.From.Id;
        var pendingRequest = _requestService.GetRequest(userId);

        if (pendingRequest == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);
            _logger.Warn(LogMessages.NotFound("Pending request", requestingUser));
            return;
        }

        _requestService.Remove(pendingRequest.Value);

        await bot.SendMessage(message.Chat.Id,
            "❌ Вы успешно отказались от обмена",
            replyParameters: replyParameters);
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }
}
