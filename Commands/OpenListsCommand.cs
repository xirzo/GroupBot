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
        await bot.DeleteMessage(message.Chat.Id, message.MessageId);

        foreach (var list in _allLists)
        {
            var text = new StringBuilder();

            text.Append($"📝 Список: {list.Name}\n\n");

            foreach (var str in list.List) text.Append(str + "\n");

            await bot.SendMessage(
                message.Chat.Id,
                text.ToString()
            );
        }
    }
}