using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class SiftCommand : ICommand
{
    private readonly IDatabaseService _database;
    private readonly ILogger _logger;

    public SiftCommand(IDatabaseService database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public long NumberOfArguments => 2;
    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
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

        var lists = await _database.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Списки не найдены.",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var list = lists.First(l => l.Name == parameters[0]);

        if (list is null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не найден список с таким именем.");
            return;
        }

        var listMembers = await _database.GetAllListMembers(list.Id);
        var member = listMembers.First(u => u.Name == parameters[1]);

        if (member is null)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Не существует человека с таким ФИО. /sift <название_списка> <фио_человека>");
            return;
        }

        try
        {
            await _database.Sift(list.Id, parameters[1]);
        }
        catch (Exception e)
        {
            _logger.Error(e);
            await bot.SendMessage(message.Chat.Id, "❌ Не удалось выполнить просеивание");
            return;
        }

        await bot.SendMessage(message.Chat.Id, "✅ Просеивание успешно выполнено");
    }
}
