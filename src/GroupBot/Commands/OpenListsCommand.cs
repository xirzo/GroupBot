using System.Text;
using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class OpenListsCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public OpenListsCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        throw new NotImplementedException();
        // await bot.DeleteMessage(message.Chat.Id, message.MessageId);
        //
        // foreach (var list in _allLists)
        // {
        //     var text = new StringBuilder();
        //
        //     text.Append($"📝 Список: {list.Name}\n\n");
        //
        //     foreach (var participant in list.List) text.Append(participant.Name + "\n");
        //
        //
        //     await bot.SendMessage(
        //         message.Chat.Id,
        //         text.ToString()
        //     );
        // }
    }
}