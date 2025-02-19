using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Library.Commands.Abstract;

public interface ICommand
{
    Task Execute(Message message, ITelegramBotClient bot);
}