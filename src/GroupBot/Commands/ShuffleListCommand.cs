using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class ShuffleListCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public ShuffleListCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/shufflelist", _])
        {
            var list = _allLists.Find(l => l.Name == words[1]);
            list?.Shuffle();

            await bot.SendMessage(message.Chat.Id, $"Список с названием {list?.Name} был зашафлен");
        }
    }
}