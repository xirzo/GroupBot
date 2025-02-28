using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class AddListCommand : ICommand
{
    private readonly IDatabaseService _db;
    private readonly ILogger _logger;

    public AddListCommand(IDatabaseService db, ILogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public string GetString() => "/addlist";

    public long NumberOfArguments => 1;

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";

        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));

        var admins = await _db.GetAllAdmins();

        if (admins.Exists(p => message.From != null && p.Id == message.From.Id) == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ У вас нет прав на выполнение этой команды",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });

            _logger.Warn(LogMessages.AccessDenied(GetString(), requestingUser));
            return;
        }

        var lists = await _db.GetAllLists();

        if (lists.Exists(l => l.Name == parameters[0]))
        {
            await bot.SendMessage(message.Chat.Id, "❌ Список с таким названием уже существует",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });

            _logger.Warn(LogMessages.ErrorOccurred(GetString(), requestingUser));
            return;
        }

        long id;

        try
        {
            id = await _db.CreateListAndShuffle(parameters[0]);
        }
        catch (Exception e)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ошибка при создании нового списка",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            _logger.Error(LogMessages.DatabaseOperationFailed(GetString(), requestingUser, requestingUser, e));
            return;
        }

        _logger.Info(LogMessages.DatabaseOperationSuccess(GetString(), requestingUser, requestingUser));

        await bot.SendMessage(message.Chat.Id, $"✅ Создан новый список с названием {parameters[0]} и id: {id}",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }
}
