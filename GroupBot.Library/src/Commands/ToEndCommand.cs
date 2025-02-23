using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;

namespace GroupBot.Library.Commands;

public class ToEndCommand : ICommand
{
    private readonly IDatabaseService _db;
    private readonly ILogger _logger;

    public ToEndCommand(IDatabaseService db, ILogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public long NumberOfArguments => 1;
    public string GetString() => "/toend";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        
        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));

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

        if (message.From == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Пользователь не найден");
            _logger.Warn(LogMessages.NotFound("message.From", requestingUser));
            return;
        }

        await _db.MoveUserToEndOfList(list.Id, message.From.Id);
        
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }

}
