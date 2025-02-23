using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class RemoveListCommand : ICommand
{
    private readonly IDatabaseService _database;
    private readonly ILogger _logger;

    public RemoveListCommand(IDatabaseService database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public long NumberOfArguments => 1;
    
    public string GetString() => "/removelist";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        
        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));
        
        try
        {
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

            var list = lists.Find(l => l.Name.Equals(parameters[0], StringComparison.OrdinalIgnoreCase));

            if (list == null)
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ Список с названием \"{parameters[0]}\" не найден.",
                    replyParameters: new ReplyParameters { MessageId = message.MessageId });
                _logger.Warn(LogMessages.NotFound(parameters[0], requestingUser));
                return;
            }

            await _database.RemoveList(list.Id);

            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: $"✅ Список \"{list.Name}\" успешно удален.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            
            _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
        }
        catch (Exception ex)
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Произошла ошибка при удалении списка.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            _logger.Error(LogMessages.DatabaseOperationFailed(GetString(), requestingUser, requestingUser, ex));
        }
        
    }
}
