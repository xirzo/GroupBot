using GroupBot.Lists;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class AddListCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public AddListCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/addlist", _])
        {
            if (_allLists.Exists(l => l.Name == words[1])) return;

            var newList = new ChatList(words[1]);

            for (var i = 0; i < ParticipantsContainer.Participants.Count; i++)
                newList.Add(ParticipantsContainer.Participants[i]);

            newList.Shuffle();
            _allLists.Add(newList);
            await bot.SendMessage(message.Chat.Id, $"Создан новый список с названием {newList.Name}");
        }
    }
}