using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot.Library.Commands;

public class StartCommand : ICommand
{
    private readonly IDatabaseService _databaseService;
    private readonly ILogger _logger;

    public StartCommand(IDatabaseService databaseService, ILogger logger)
    {
        _databaseService = databaseService;
        _logger = logger;
    }

    public long NumberOfArguments => 0;

    public string GetString() => "/start";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";

        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));

        if (message.Chat.Type != ChatType.Private)
        {
            await bot.SendMessage(message.Chat.Id, "❌ Используйте команду в личном чате",
                replyParameters: new ReplyParameters { MessageId = message.MessageId });
            _logger.Info(LogMessages.ErrorOccurred(GetString(), $"{requestingUser} used {GetString()} not in dms", null));
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
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }
}
