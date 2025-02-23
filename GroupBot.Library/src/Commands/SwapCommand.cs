using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot.Library.Commands;

public class SwapCommand : ICommand
{
    private readonly IRequestService _requestService;
    private readonly IDatabaseService _db;
    private readonly ILogger _logger;

    public SwapCommand(IRequestService requestService, IDatabaseService db, ILogger logger)
    {
        _requestService = requestService;
        _db = db;
        _logger = logger;
    }

    public long NumberOfArguments => 1;

    public string GetString() => "/swap";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        
        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));
        
        var replyParameters = new ReplyParameters
        {
            MessageId = message.MessageId
        };

        var user = message.From;

        if (user == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Пользователь запрашивающий команду не найден",
                replyParameters: replyParameters);
            _logger.Warn(LogMessages.NotFound(requestingUser, requestingUser));
            return;
        }

        var replyToMessage = message.ReplyToMessage;

        if (replyToMessage == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Вам нужно ответить на сообщение человека из списка команд",
                replyParameters: replyParameters);
            _logger.Warn(LogMessages.ErrorOccurred("Must reply to person from list", requestingUser));
            return;
        }

        var lists = await _db.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            _logger.Warn(LogMessages.NotFound("Lists", requestingUser));
            return;
        }

        var list = lists.Find(l => l.Name == parameters[0]);
        
        if (list is null)
        { 
            await bot.SendMessage(message.Chat.Id, "❌ Не найден список с таким именем."); 
            _logger.Warn(LogMessages.NotFound(parameters[0], requestingUser));
            return;
        }

        var targetUser = replyToMessage.From;

        if (targetUser == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не удалось определить пользователя для обмена",
                replyParameters: replyParameters);
            _logger.Warn(LogMessages.ErrorOccurred("targetUser is null", requestingUser));
            return;
        }

        if (targetUser == user)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Вы не можете отправить запрос самому себе",
                replyParameters: replyParameters);
            _logger.Warn(LogMessages.ErrorOccurred("Cannot swap with self", requestingUser));
            return;
        }

        var userDbId = await _db.GetUserIdByTelegramId(user.Id);
        var targetUserDbId = await _db.GetUserIdByTelegramId(targetUser.Id);

        var pendingRequest = new PendingRequest(targetUser.Id, userDbId, targetUserDbId, list.Id);

        _requestService.Add(pendingRequest);

        var replyMarkup = new ReplyKeyboardMarkup(true).AddButtons("Принять", "Отказаться");

        await bot.SendMessage(targetUser.Id,
            $"📝 {user.Username} отправил тебе swap-запрос в списке {list.Name}",
            replyMarkup: replyMarkup);
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, targetUser.Username));
    }
}
