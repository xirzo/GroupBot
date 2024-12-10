using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class ListCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public ListCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/list", _])
        {
            var list = _allLists.Find(l => l.Name == words[1]);

            if (list != null)
            {
                var listContains = string.Join("\n", list.List.Select(l => l));
                await bot.SendMessage(message.Chat.Id, listContains);
            }
        }
    }
}