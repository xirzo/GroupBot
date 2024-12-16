using GroupBot.Commands.Abstract;
using GroupBot.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class AddListCommand : ICommand
{
    private readonly DatabaseHelper _db;

    public AddListCommand(DatabaseHelper db)
    {
        _db = db;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');


        if (words is ["/addlist", _] == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /addlist <название списка>",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var lists = await _db.GetAllLists();

        if (lists.Exists(l => l.Name == words[1]))
        {
            await bot.SendMessage(message.Chat.Id, "❌ Список с таким названием уже существует",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var id = _db.CreateListAndShuffle(words[1]);
        await bot.SendMessage(message.Chat.Id, $"Создан новый список с названием {words[1]} и id: {id}");
    }
}