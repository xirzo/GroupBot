using System.Text;
using GroupBot.Library.Commands.Abstract;
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

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/list", _] == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /list <название списка>",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var lists = await _db.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            return;
        }

        var list = lists.First(l => l.Name == words[1]);

        var participants = await _db.GetAllParticipantsInList(list.Id);

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