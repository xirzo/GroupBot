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

        if (words.Length > 2 && words[0] == "/addtolist")
        {
            var list = _allLists.Find(l => l.Name == words[1]);

            if (list != null)
            {
                for (var i = 2; i < words.Length; i++) list.Add(words[i]);

                await bot.SendMessage(message.Chat.Id, $"В список с названием {list.Name} добавлены новые люди");
            }
        }
    }
}