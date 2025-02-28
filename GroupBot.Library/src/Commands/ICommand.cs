using Telegram.Bot;

namespace GroupBot.Library.Commands;

public interface ICommand
{
    string GetString();
    long NumberOfArguments { get; }
    Task Execute(ValidatedMessage message, ITelegramBotClient bot, string[] parameters);
}
