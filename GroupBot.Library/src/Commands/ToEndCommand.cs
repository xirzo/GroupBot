using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ToEndCommand : ICommand
{
  private IDatabaseService _db;

  public ToEndCommand(IDatabaseService db)
  {
    _db = db;
  }

  public async Task Execute(Message message, TelegramBotClient bot)
  {
    if (message.From == null)
    {
      throw new Exception("There is no message");
    }

    var words = message.Text?.Split(' ');

    if (words is ["/toend", _] == false)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /toend <название списка>",
          replyParameters: new ReplyParameters { MessageId = message.MessageId });
      return;
    }

    var lists = await _db.GetAllLists();

    if (lists.Count == 0)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
      return;
    }

    var list = lists.First(l => l.Name == words[1]);

    await _db.MoveUserToEndOfList(list.Id, message.From.Id);
  }
}
