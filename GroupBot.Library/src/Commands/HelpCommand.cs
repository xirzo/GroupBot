using GroupBot.Library.Commands;
using Telegram.Bot;
using Telegram.Bot.Types;

public class HelpCommand : ICommand
{
    public long NumberOfArguments => 0;

    public async Task Execute(Message message, ITelegramBotClient bot, string[] parameters)
    {
        await bot.SendMessage(
            message.Chat.Id,
            "Команды:\n\n - Чтобы увидеть созданные списки напиши команду /lists\n - Чтобы увидеть конкретный список напиши /list <название_списка>\n - Чтобы отправиться в конец очереди напиши /toend <название_списка>\n - Чтобы обменяться местами напиши /swap <название_списка> и ответь на сообщение человека, с которым хочешь поменяться"
        );
    }
}
