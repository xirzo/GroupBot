using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class RemoveFromListCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public RemoveFromListCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words == null) return;

        if (words is ["/removefromlist", _, _])
        {
            var list = _allLists.Find(l => l.Name == words[1]);

            if (list != null)
            {
                list.Remove(long.Parse(words[2]));

                await bot.SendMessage(message.Chat.Id, $"Из списка с названием {list.Name} удалены люди");
            }
        }
    }
}