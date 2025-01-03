using GroupBot.Commands.Abstract;
using GroupBot.Requests;
using GroupBot.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot.Commands;

public class SwapCommand : ICommand
{
  private readonly RequestsContainer _requestsContainer;
  private readonly IDatabaseService _db;

  public SwapCommand(RequestsContainer requestsContainer, IDatabaseService db)
  {
    _requestsContainer = requestsContainer;
    _db = db;
  }

  public async Task Execute(Message message, TelegramBotClient bot)
  {
    var words = message.Text?.Split(' ');


    var replyParameters = new ReplyParameters
    {
      MessageId = message.MessageId
    };

    if (words is ["/swap", _] == false)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /swap <название списка>",
          replyParameters: new ReplyParameters { MessageId = message.MessageId });
      return;
    }


    var user = message.From;

    if (user == null)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Пользователь запрашивающий команду не найден",
          replyParameters: replyParameters);

      return;
    }

    var replyToMessage = message.ReplyToMessage;

    if (replyToMessage == null)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Вам нужно ответить на сообщение человека из списка команд",
          replyParameters: replyParameters);
      return;
    }

    var lists = await _db.GetAllLists();

    if (lists.Count == 0)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
      return;
    }

    var list = lists.First(l => l.Name == words[1]);

    var targetUser = replyToMessage.From;

    if (targetUser == null)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Не удалось определить пользователя для обмена",
          replyParameters: replyParameters);
      return;
    }

    if (targetUser == user)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Вы не можете отправить запрос самому себе",
          replyParameters: replyParameters);
      return;
    }

    var userDbId = await _db.GetParticipantIdByTelegramId(user.Id);
    var targetUserDbId = await _db.GetParticipantIdByTelegramId(targetUser.Id);

    var pendingRequest = new PendingRequest(targetUser.Id, userDbId, targetUserDbId, list.Id);

    _requestsContainer.Add(pendingRequest);

    var replyMarkup = new ReplyKeyboardMarkup(true).AddButtons("Принять", "Отказаться");

    await bot.SendMessage(targetUser.Id,
        $"{user.Username} отправил тебе swap-запрос в списке {list.Name}",
        replyMarkup: replyMarkup);
  }
}
