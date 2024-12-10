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

        if (words.Length > 2 && words[0] == "/removefromlist")
        {
            var list = _allLists.Find(l => l.Name == words[1]);

            if (list != null)
            {
                for (var i = 2; i < words.Length; i++) list.Remove(words[i]);

                await bot.SendMessage(message.Chat.Id, $"Из списка с названием {list.Name} удалены люди");
            }
        }
    }
}