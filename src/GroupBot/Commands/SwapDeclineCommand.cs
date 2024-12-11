using GroupBot.Lists;
using GroupBot.Requests;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Commands;

public class SwapDeclineCommand : ICommand
{
    private readonly List<ChatList> _allLists;

    public SwapDeclineCommand(List<ChatList> allLists)
    {
        _allLists = allLists;
    }

    public Task Execute(Message message, TelegramBotClient bot)
    {
        var replyParameters = new ReplyParameters
        {
            MessageId = message.MessageId
        };

        if (message.Chat.Type != ChatType.Private)
            return bot.SendMessage(message.Chat.Id, "❌ Ответьте на сообщение в личном чате",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });

        if (message.From == null)
            return bot.SendMessage(message.Chat.Id, "❌ Пользователь не найден", replyParameters: replyParameters);

        var userId = message.From.Id;
        var pendingRequest = PendingRequestsContainer.GetRequest(userId);

        if (pendingRequest == null)
            return bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);

        var listName = pendingRequest.Value.ListName;

        var list = _allLists.FirstOrDefault(list => list.Name == listName);

        if (list == null)
            return bot.SendMessage(message.Chat.Id, "❌ Список не найден", replyParameters: replyParameters);


        PendingRequestsContainer.RemoveRequest(pendingRequest.Value);

        return bot.SendMessage(message.Chat.Id,
            $"❌ Вы успешно отказались от обмена в списке {list.Name}",
            replyParameters: replyParameters);
    }
}