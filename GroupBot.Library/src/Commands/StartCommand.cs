using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot.Library.Commands;

public class StartCommand : ICommand
{
    private readonly IDatabaseService _databaseService;

    public StartCommand(IDatabaseService databaseService)
    {
        _databaseService = databaseService;
    }

    public long NumberOfArguments => 0;

    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
    {
        if (message.Chat.Type != ChatType.Private)
        {
            return;
        }

        var lists = await _databaseService.GetAllLists();
        var keyboardButtons = new KeyboardButton[lists.Count];

        for (var i = 0; i < lists.Count; i++)
        {
            keyboardButtons[i] = new KeyboardButton("/list " + lists[i].Name);
        }

        var replyMarkup = new ReplyKeyboardMarkup(true).AddButtons(keyboardButtons);

        await bot.SendMessage(message.Chat.Id, "Привет! Выбери список для отрисовки", replyMarkup: replyMarkup);
        return;
    }
}
