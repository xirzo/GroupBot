using GroupBot.Commands.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public class AddListCommand(Database.DatabaseHelper db) : ICommand
{
    public async Task Execute(Message message, TelegramBotClient bot)
    {
        var words = message.Text?.Split(' ');

        if (words is ["/addlist", _])
        {
            var id = db.CreateListAndShuffle(words[1]);
            await bot.SendMessage(message.Chat.Id, $"Создан новый список с названием {words[1]} и id: {id}");
        }
    }
}