using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class AddToListCommand : ICommand
{
    private readonly Database.Database _db;

    public AddToListCommand(Database.Database db)
    {
        _db = db;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/addtolist", _, _])
        {
            var lists = await _db.GetAllLists();

            var list = lists.First(l => l.Name == words[1]);

            if (list == null)
                throw new ArgumentException($"List {words[1]} not found");

            var doesUserExist = await _db.DoesUserExist(long.Parse(words[2]));

            if (doesUserExist == false)
            {
                await bot.SendMessage(message.Chat.Id, $"Пользователь с айди {words[2]} не найден");
                return;
            }

            _db.TryAddUserToList(list.Id, long.Parse(words[2]));

            await bot.SendMessage(message.Chat.Id, $"Участник {words[2]} добавлен в список {list.Name}");
        }
    }
}