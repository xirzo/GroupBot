using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class SiftCommand : ICommand
{
    private readonly IDatabaseService _database;
    private readonly ILogger _logger;

    public SiftCommand(IDatabaseService database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public long NumberOfArguments => 2;

    public string GetString() => "/sift";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        var targetUser = parameters[0];

        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, targetUser, message.Chat.Id));

        var admins = await _database.GetAllAdmins();

        if (admins.Exists(p => message.From != null && p.Id == message.From.Id) == false)
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ У вас нет прав на выполнение этой команды",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });

            _logger.Warn(LogMessages.AccessDenied(GetString(), requestingUser));
            return;
        }

        var lists = await _database.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Списки не найдены.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });

            _logger.Warn(LogMessages.NotFound("Lists", requestingUser));
            return;
        }

        var list = lists.Find(l => l.Name == parameters[0]);

        if (list is null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не найден список с таким именем.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId }
            );
            _logger.Warn(LogMessages.NotFound(parameters[0], requestingUser));
            return;
        }

        var listMembers = await _database.GetAllListMembers(list.Id);
        var member = listMembers.Find(u => u.Name == parameters[1]);

        if (member is null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не существует человека с таким ФИО. /sift <название_списка> <фио_человека>",
                replyParameters: new ReplyParameters { MessageId = message.MessageId }
            );
            _logger.Warn(LogMessages.NotFound(parameters[1], requestingUser));
            return;
        }

        try
        {
            await _database.Sift(list.Id, parameters[1]);
        }
        catch (Exception ex)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не удалось выполнить просеивание",
                replyParameters: new ReplyParameters { MessageId = message.MessageId }
            );
            _logger.Error(LogMessages.DatabaseOperationFailed(GetString(), requestingUser, requestingUser, ex));
            return;
        }

        await bot.SendMessage(message.Chat.Id, "✅ Просеивание успешно выполнено",
            replyParameters: new ReplyParameters { MessageId = message.MessageId }
        );
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, targetUser));
    }
}
