using GroupBot.Lists;
using GroupBot.Requests;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Commands;

public class SwapAcceptCommand : ICommand
{
    private readonly RequestsContainer _requestsContainer;
    private readonly Database.Database _db;

    public SwapAcceptCommand(RequestsContainer requestsContainer, Database.Database db)
    {
        _requestsContainer = requestsContainer;
        _db = db;
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

        var userTelegramId = message.From.Id;
        var pendingRequest = _requestsContainer.GetRequest(userTelegramId);

        if (pendingRequest == null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);
            return;
        }

        var lists = await _db.GetAllLists();

        var list = lists.First(l => l.Id == pendingRequest.Value.ListDbId);

        list.Swap(pendingRequest.Value.UserDbId, pendingRequest.Value.TargetUserDbId);

        _requestsContainer.Remove(pendingRequest.Value);

        await bot.SendMessage(message.Chat.Id,
            $"✅ Вы были успешно обменены местами в списке {list.Name}",
            replyParameters: replyParameters);
    }
}