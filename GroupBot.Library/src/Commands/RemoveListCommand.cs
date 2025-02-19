using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Services.Database;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class RemoveListCommand : ICommand
{
    private readonly IDatabaseService _database;

    public RemoveListCommand(IDatabaseService database)
    {
        _database = database;
    }

    public async Task Execute(Message message, ITelegramBotClient bot)
    {
        try
        {
            var admins = await _database.GetAllAdmins();

            if (admins.Exists(p => message.From != null && p.Id == message.From.Id) == false)
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ У вас нет прав на выполнение этой команды",
                    replyParameters: new ReplyParameters { MessageId = message.MessageId });
                return;
            }

            var words = message.Text?.Split(' ');

            if (words is not ["/removelist", var listName])
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ Неверный формат команды. Используйте /removelist <название списка>",
                    replyParameters: new ReplyParameters { MessageId = message.MessageId });
                return;
            }

            var lists = await _database.GetAllLists();

            if (lists.Count == 0)
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: "❌ Списки не найдены.",
                    replyParameters: new ReplyParameters { MessageId = message.MessageId });
                return;
            }

            var list = lists.FirstOrDefault(l => l.Name.Equals(listName, StringComparison.OrdinalIgnoreCase));
            
            if (list == null)
            {
                await bot.SendMessage(
                    chatId: message.Chat.Id,
                    text: $"❌ Список с названием \"{listName}\" не найден.",
                    replyParameters: new ReplyParameters { MessageId = message.MessageId });
                return;
            }

            await _database.RemoveList(list.Id);

            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: $"✅ Список \"{list.Name}\" успешно удален.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
        }
        catch (Exception ex)
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Произошла ошибка при удалении списка.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            
            Console.WriteLine(ex);
        }
    }
}