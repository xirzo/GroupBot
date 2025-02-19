using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Services.Request;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Commands;

public class SwapDeclineCommand : ICommand
{
    private readonly IRequestService _requestService;

    public SwapDeclineCommand(IRequestService requestService)
    {
        _requestService = requestService;
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
        var pendingRequest = _requestService.GetRequest(userId);

        if (pendingRequest == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);
            return;
        }

        _requestService.Remove(pendingRequest.Value);

        await bot.SendMessage(message.Chat.Id,
            "❌ Вы успешно отказались от обмена",
            replyParameters: replyParameters);
        return;
    }
}