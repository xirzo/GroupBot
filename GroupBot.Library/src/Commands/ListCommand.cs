using System.Text;
using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ListCommand : ICommand
{
    private readonly IDatabaseService _db;
    private readonly ILogger _logger;

    public ListCommand(IDatabaseService db, ILogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public string GetString() => "/list";

    public long NumberOfArguments => 1;

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

        var participants = await _db.GetAllListMembers(list.Id);

        var text = new StringBuilder();

        text.Append($"📝 Список: {list.Name}\n\n");

        var position = 1;

        foreach (var participant in participants)
        {
            text.Append(position + ". " + participant.Name + "\n");
            position++;
        }

        await bot.SendMessage(
            message.Chat.Id,
            text.ToString()
        );
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }
}
