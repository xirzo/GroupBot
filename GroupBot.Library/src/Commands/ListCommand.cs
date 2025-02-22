using System.Text;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ListCommand : ICommand
{
    private readonly IDatabaseService _db;

    public ListCommand(IDatabaseService db)
    {
        _db = db;
    }

    public long NumberOfArguments => 1;

    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
    {
        var lists = await _db.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            return;
        }

        var list = lists.First(l => l.Name == parameters[0]);

        if (list is null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не найден список с таким именем.");
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
    }
}
