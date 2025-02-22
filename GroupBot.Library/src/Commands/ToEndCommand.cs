using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands;

public class ToEndCommand : ICommand
{
    private readonly IDatabaseService _db;

    public ToEndCommand(IDatabaseService db)
    {
        _db = db;
    }

    public long NumberOfArguments => 1;

    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
    {
        if (message.From == null)
        {
            throw new Exception("There is no message");
        }

        var lists = await _db.GetAllLists();

        if (lists.Count == 0)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Списки не найдены.");
            return;
        }

        var list = lists.First(l => l.Name == parameters[0]);

        await _db.MoveUserToEndOfList(list.Id, message.From.Id);
    }

}
