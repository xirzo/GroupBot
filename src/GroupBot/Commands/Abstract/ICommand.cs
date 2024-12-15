using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands.Abstract;

public interface ICommand
{
    Task Execute(Message message, TelegramBotClient bot);
}