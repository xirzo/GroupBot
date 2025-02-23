using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Commands;

public class SwapAcceptCommand : ICommand
{
    private readonly IRequestService _requestService;
    private readonly IDatabaseService _db;
    private readonly ILogger _logger;

    public SwapAcceptCommand(IRequestService requestService, IDatabaseService db, ILogger logger)
    {
        _requestService = requestService;
        _db = db;
        _logger = logger;
    }

    public long NumberOfArguments => 0;
    
    public string GetString() => "Принять";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        var targetUser = message.From;        
        
        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, targetUser?.Username, message.Chat.Id));
        
        var replyParameters = new ReplyParameters
        {
            MessageId = message.MessageId
        };

        if (message.Chat.Type != ChatType.Private)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ответьте на сообщение в личном чате",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        if (targetUser == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Пользователь не найден", replyParameters: replyParameters);
            _logger.Warn(LogMessages.NotFound("message.From", requestingUser));
            return;
        }

        var userTelegramId = targetUser.Id;
        var pendingRequest = _requestService.GetRequest(userTelegramId);

        if (pendingRequest == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);
            _logger.Warn(LogMessages.NotFound("Pending request", requestingUser));
            return;
        }

        var lists = await _db.GetAllLists();

        var list = lists.Find(l => l.Id == pendingRequest.Value.ListDbId);
        
        if (list is null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не найден список с таким айди.");
            _logger.Warn(LogMessages.NotFound(pendingRequest.Value.ListDbId.ToString(), requestingUser));
            return;
        }

        await list.SwapAsync(pendingRequest.Value.UserDbId, pendingRequest.Value.TargetUserDbId, _db);

        _requestService.Remove(pendingRequest.Value);

        await bot.SendMessage(message.Chat.Id,
            $"✅ Вы были успешно обменены местами в списке {list.Name}",
            replyParameters: replyParameters);
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, targetUser.Username));
    }
}
