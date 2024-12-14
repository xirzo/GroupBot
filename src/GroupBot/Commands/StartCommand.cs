using GroupBot.Commands.Abstract;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot.Commands;

public class StartCommand : ICommand
{
    public async Task Execute(Message message, TelegramBotClient bot)
    {
        if (message.Chat.Type == ChatType.Private)
        {
            var replyMarkup = new ReplyKeyboardMarkup(true).AddButtons("/lists");
            await bot.SendMessage(message.Chat.Id, "Привет! Выбери команду", replyMarkup: replyMarkup);
            return;
        }

        await bot.SendMessage(
            message.Chat.Id,
            "Привет!\n - Чтобы увидеть созданные списки напиши команду /lists\n - Чтобы добавить новый список пропиши команду /addlist <название_списка>\n - Чтобы увидеть конкретный список напиши /list <название_списка>\n - Чтобы отправиться в конец очереди напиши /toend <название_списка>"
        );
    }
}