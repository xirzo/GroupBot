using GroupBot.Lists;
using GroupBot.Requests;
using GroupBot.Shared;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot.Commands;

public class SwapCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public SwapCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/swap", _] == false)
            return bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /swap <название списка>",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });


        var replyParameters = new ReplyParameters
        {
            MessageId = message.MessageId
        };

        var participants = ParticipantsContainer.Participants;

        var user = message.From;
        if (user == null)
            return bot.SendMessage(message.Chat.Id, "❌ Пользователь запрашивающий команду не найден",
                replyParameters: replyParameters);

        var replyToMessage = message.ReplyToMessage;

        if (replyToMessage == null)
            return bot.SendMessage(message.Chat.Id, "❌ Вам нужно ответить на сообщение человека из списка команд",
                replyParameters: replyParameters);


        var list = _allLists.Find(l => l.Name == words[1]);

        if (list == null)
            return bot.SendMessage(message.Chat.Id, "❌ Список не найден", replyParameters: replyParameters);

        var targetUser = replyToMessage.From;

        if (targetUser == null)
            return bot.SendMessage(message.Chat.Id, "❌ Не удалось определить пользователя для обмена",
                replyParameters: replyParameters);

        var participantAsker = participants.Find(p => p.Id == user.Id);
        var participantGiver = participants.Find(p => p.Id == targetUser.Id);

        if (participantAsker == null || participantGiver == null)
            return bot.SendMessage(message.Chat.Id, "❌ Один из пользователей не найден в списке участников",
                replyParameters: replyParameters);

        var pendingRequest = new PendingRequest
        {
            UserId = participantAsker.Id,
            TargetUserId = participantGiver.Id,
            ListName = list.Name
        };

        PendingRequestsContainer.AddRequest(pendingRequest);

        var replyMarkup = new ReplyKeyboardMarkup(true).AddButtons("Принять", "Отказаться");

        return bot.SendMessage(participantGiver.Id,
            $"{user.Username} отправил тебе swap-запрос в списке {list.Name}",
            replyMarkup: replyMarkup);
    }
}