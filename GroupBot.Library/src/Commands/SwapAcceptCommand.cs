using System.Threading.Tasks;
using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot.Library.Commands;

public class SwapAcceptCommand : ICommand
{
  private readonly IRequestService _requestService;
  private readonly IDatabaseService _db;

  public SwapAcceptCommand(IRequestService requestService, IDatabaseService db)
  {
    _requestService = requestService;
    _db = db;
  }

  public async Task Execute(Message message, ITelegramBotClient bot)
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
    var pendingRequest = _requestService.GetRequest(userTelegramId);

    if (pendingRequest == null)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Ожидающий запрос не найден.", replyParameters: replyParameters);
      return;
    }

    var lists = await _db.GetAllLists();

    var list = lists.First(l => l.Id == pendingRequest.Value.ListDbId);

    await list.SwapAsync(pendingRequest.Value.UserDbId, pendingRequest.Value.TargetUserDbId, _db);

    _requestService.Remove(pendingRequest.Value);

    await bot.SendMessage(message.Chat.Id,
        $"✅ Вы были успешно обменены местами в списке {list.Name}",
        replyParameters: replyParameters);
  }
}
