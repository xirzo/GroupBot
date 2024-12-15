using GroupBot.Commands.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class ToEndCommand : ICommand
{
    private Database.DatabaseHelper _db;

    public ToEndCommand(Database.DatabaseHelper db)
    {
        _db = db;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        if (message.From == null)
        {
            throw new Exception("There is no message");
        }

        var words = message.Text?.Split(' ');

        if (words is ["/toend", _] == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /toend <название списка>",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var lists = await _db.GetAllLists();

        var list = lists.First(l => l.Name == words[1]);

        await _db.MoveUserToEndOfListAsync(list.Id, message.From.Id);
    }
}