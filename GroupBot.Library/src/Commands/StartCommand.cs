using System.Threading.Tasks;
using GroupBot.Library.Commands.Abstract;
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

    public async Task Execute(Message message, ITelegramBotClient bot)
    {
        if (message.Chat.Type == ChatType.Private)
        {
            var lists = await _databaseService.GetAllLists();
            var keyboardButtons = new KeyboardButton[lists.Count];

            for (var i = 0; i < lists.Count; i++) keyboardButtons[i] = new KeyboardButton("/list " + lists[i].Name);

            var replyMarkup = new ReplyKeyboardMarkup(true).AddButtons(keyboardButtons);
            await bot.SendMessage(message.Chat.Id, "Привет! Выбери команду", replyMarkup: replyMarkup);
            return;
        }

        await bot.SendMessage(
            message.Chat.Id,
            "Привет!\n\n - Чтобы увидеть созданные списки напиши команду /lists\n - Чтобы добавить новый список пропиши команду /addlist <название_списка>\n - Чтобы удалить список пропиши команду /removelist <название_списка>\n - Чтобы увидеть конкретный список напиши /list <название_списка>\n - Чтобы отправиться в конец очереди напиши /toend <название_списка>\n - Чтобы обменяться местами напиши /swap <название_списка> и ответь на сообщение человека, с которым хочешь поменяться"
        );
    }
}