using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class ListsCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public ListsCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        if (message.Text == "/lists")
        {
            var allLists = string.Join("\n", _allLists.Select(l => l.Name));
            await bot.SendMessage(message.Chat.Id, allLists);
        }
    }
}