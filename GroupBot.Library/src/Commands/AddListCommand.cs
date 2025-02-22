using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class AddListCommand : ICommand
{
    private readonly IDatabaseService _db;

    public AddListCommand(IDatabaseService db)
    {
        _db = db;
    }

    public long NumberOfArguments => 1;

    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
    {
        var admins = await _db.GetAllAdmins();

        if (admins.Exists(p => message.From != null && p.Id == message.From.Id) == false)
        {
            await bot.SendMessage(message.Chat.Id, "❌ У вас нет прав на выполнение этой команды",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var lists = await _db.GetAllLists();

        if (lists.Exists(l => l.Name == parameters[0]))
        {
            await bot.SendMessage(message.Chat.Id, "❌ Список с таким названием уже существует",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            return;
        }

        var id = await _db.CreateListAndShuffle(parameters[0]);

        await bot.SendMessage(message.Chat.Id, $"✅ Создан новый список с названием {parameters[0]} и id: {id}");
    }
}
