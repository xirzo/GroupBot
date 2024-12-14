using System.Text;
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
        throw new NotImplementedException();
        // var words = message.Text?.Split(' ');
        //
        // if (words is ["/list", _])
        // {
        //     var list = _allLists.Find(l => l.Name == words[1]);
        //
        //     if (list == null) return;
        //
        //     var text = new StringBuilder();
        //
        //     text.Append($"📝 Список: {list.Name}\n\n");
        //
        //     foreach (var participant in list.List) text.Append(participant.Name + "\n");
        //
        //     await bot.SendMessage(
        //         message.Chat.Id,
        //         text.ToString()
        //     );
    }
}