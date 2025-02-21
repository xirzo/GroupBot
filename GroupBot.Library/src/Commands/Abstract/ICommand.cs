using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands.Abstract;

public interface ICommand
{
    long NumberOfArguments { get; }
    Task Execute(Message message, ITelegramBotClient bot, string[] parameters);
}
