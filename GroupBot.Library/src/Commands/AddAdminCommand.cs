using GroupBot.Library.Commands;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

public class AddAdminCommand : ICommand
{
    private readonly IDatabaseService _database;

    public AddAdminCommand(IDatabaseService database)
    {
        _database = database;
    }

    public long NumberOfArguments => 1;

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

        var userId = await _database.GetUserIdByFullName(parameters[0]);

        try
        {
            await _database.AddAdmin(userId);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            await bot.SendMessage(
                chatId: message.Chat.Id,
                text: "❌ Ошибка при добавлении нового админа",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        await bot.SendMessage(
            chatId: message.Chat.Id,
            text: "✅ Новый админ успешно добавлен",
            replyParameters: new ReplyParameters { MessageId = message.MessageId });
    }
}
