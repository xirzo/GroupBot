using GroupBot.Commands.Abstract;
using GroupBot.Requests;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Commands;

public class SwapDeclineCommand : ICommand
{
    private readonly RequestsContainer _requestsContainer;

    public SwapDeclineCommand(RequestsContainer requestsContainer)
    {
        _requestsContainer = requestsContainer;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var replyParameters = new ReplyParameters
        {
            MessageId = message.MessageId
        };

        if (message.Chat.Type != ChatType.Private)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ответьте на сообщение в личном чате",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        if (message.From == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Пользователь не найден", replyParameters: replyParameters);
            return;
        }

        var userId = message.From.Id;
        var pendingRequest = _requestsContainer.GetRequest(userId);

        if (pendingRequest == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);
            return;
        }

        _requestsContainer.Remove(pendingRequest.Value);

        await bot.SendMessage(message.Chat.Id,
            "❌ Вы успешно отказались от обмена",
            replyParameters: replyParameters);
        return;
    }
}