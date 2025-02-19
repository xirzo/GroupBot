using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class RemoveListCommand : ICommand
{
    private readonly IDatabaseService _database;

    public RemoveListCommand(IDatabaseService database)
    {
        _database = database;
    }

    public async Task Execute(Message message, ITelegramBotClient bot)
    {
        var admins = await _database.GetAllAdmins();

        if (admins.Exists(p => message.From != null && p.Id == message.From.Id) == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ У вас нет прав на выполнение этой команды",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var words = message.Text?.Split(' ');

        if (words is ["/removelist", _] == false)
        {
            await bot.SendMessage(message.Chat.Id,
                "❌ Неверный формат команды. Используйте /removelist <название списка>",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var lists = await _database.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            return;
        }

        var list = lists.First(l => l.Name == words[1]);
        await _database.RemoveList(list.Id);

        await bot.SendMessage(
            message.Chat.Id,
            $"Список {list.Name} успешно удален."
        );
    }
}