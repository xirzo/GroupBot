using System.Text;
using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ListsCommand : ICommand
{
    private readonly IDatabaseService _db;
    private readonly ILogger _logger;

    public ListsCommand(IDatabaseService db, ILogger logger)
    {
        _db = db;
        _logger = logger;
    }

    public long NumberOfArguments => 0;

    public string GetString() => "/lists";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";

        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));

        var lists = await _db.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId }
            );
            _logger.Warn(LogMessages.NotFound("Lists", requestingUser));
            return;
        }

        var text = new StringBuilder();
        text.Append($"📝 Списки:\n\n");
        text.Append(string.Join("\n", lists.Select(l => l.Name)));
        await bot.SendMessage(message.Chat.Id, text.ToString(),
            replyParameters: new ReplyParameters { MessageId = message.MessageId }
        );
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }
}
