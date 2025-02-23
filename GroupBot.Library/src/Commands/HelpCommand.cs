using GroupBot.Library.Commands;
using GroupBot.Library.Logging;
using Telegram.Bot;

public class HelpCommand : ICommand
{
    private readonly ILogger _logger;

    public HelpCommand(ILogger logger)
    {
        _logger = logger;
    }

    public long NumberOfArguments => 0;
    
    public string GetString() => "/help";

    public async Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters)
    {
        var requestingUser = message.From?.Username ?? "unknown";
        
        _logger.Info(LogMessages.CommandStarted(GetString(), requestingUser, null, message.Chat.Id));
        
        await bot.SendMessage(
            message.Chat.Id,
            "Команды:\n\n - Чтобы увидеть созданные списки напиши команду /lists\n - Чтобы увидеть конкретный список напиши /list <название_списка>\n - Чтобы отправиться в конец очереди напиши /toend <название_списка>\n - Чтобы обменяться местами напиши /swap <название_списка> и ответь на сообщение человека, с которым хочешь поменяться"
        );
        
        _logger.Info(LogMessages.CommandCompleted(GetString(), requestingUser, null));
    }
}
