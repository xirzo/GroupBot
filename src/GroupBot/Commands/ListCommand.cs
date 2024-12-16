using System.Text;
using GroupBot.Commands.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class ListCommand : ICommand
{
  private readonly Database.DatabaseHelper _db;

  public ListCommand(Database.DatabaseHelper db)
  {
    _db = db;
  }

  public async Task Execute(Message message, TelegramBotClient bot)
  {
    var words = message.Text?.Split(' ');

    if (words is ["/list", _] == false)
    {
      await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /list <название списка>",
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

    var users = await _db.GetAllUsersInList(list.Id);

    var text = new StringBuilder();

    text.Append($"📝 Список: {list.Name}\n\n");

    foreach (var user in users)
    {
      text.Append(user.Position + ". " + user.Name + "\n");
    }

    await bot.SendMessage(
        message.Chat.Id,
        text.ToString()
    );
  }
}
