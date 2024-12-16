using System.Text;
using GroupBot.Commands.Abstract;
using GroupBot.Lists;
using Moq;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class ListsCommand : ICommand
{
    private readonly Database.DatabaseHelper _db;

    public ListsCommand(Database.DatabaseHelper db)
    {
        _db = db;
    }

    public async Task Execute(Message message, TelegramBotClient bot)
    {
        if (message.Text == "/lists" == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Неверный формат команды. Используйте /lists");
            return;
        }
        
        var lists = await _db.GetAllLists();
        
        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            return;
        } 

        var text = new StringBuilder();
        text.Append($"📝 Списки:\n\n");
        text.Append(string.Join("\n", lists.Select(l => l.Name)));
        await bot.SendMessage(message.Chat.Id, text.ToString());
    }
}