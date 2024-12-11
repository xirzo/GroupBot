using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class AddToListCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public AddToListCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words == null) return;

        if (words is ["/addtolist", _, _])
        {
            var list = _allLists.Find(l => l.Name == words[1]);

            if (list != null)
            {
                list.Add(long.Parse(words[2]), words[1]);

                await bot.SendMessage(message.Chat.Id, $"В список с названием {list.Name} добавлены новые люди");
            }
        }
    }
}