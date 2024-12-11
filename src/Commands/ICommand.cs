using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Commands;

public interface ICommand
{
    Task Execute(Message message, TelegramBotClient bot);
}