using System.Text;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ListsCommand : ICommand
{
    private readonly IDatabaseService _db;
    public long NumberOfArguments => 0;

    public ListsCommand(IDatabaseService db)
    {
        _db = db;
    }

    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
    {
        var lists = await _db.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            return;
        }

        var text = new StringBuilder();
        text.Append($"📝 Списки:\n\n");
        text.Append(string.Join("\n", lists.Select(l => l.Name)));
        await bot.SendMessage(message.Chat.Id, text.ToString());
    }
}
