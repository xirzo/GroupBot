using GroupBot.Commands;
using GroupBot.Lists;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

var allLists = new List<ChatList>();
var factory = new CommandFactory();
TelegramBotClient bot = null!;

IConfiguration config = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
    .Build();

var botToken = config.GetSection("BotConfiguration")["BotToken"];

if (string.IsNullOrEmpty(botToken))
    throw new ArgumentException("Bot token environment variable is missing");

using var cts = new CancellationTokenSource();
bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);

if (bot == null)
    throw new ArgumentException("Bot is null");

var me = await bot.GetMe(cts.Token);

factory.Register("/start", new StartCommand());
factory.Register("/addlist", new AddListCommand(allLists));
factory.Register("/addtolist", new AddToListCommand(allLists));
factory.Register("/shufflelist", new ShuffleListCommand(allLists));
factory.Register("/removefromlist", new RemoveFromListCommand(allLists));
factory.Register("/list", new ListCommand(allLists));
factory.Register("/lists", new ListsCommand(allLists));
factory.Register("/openlists", new OpenListsCommand(allLists));
factory.Register("/swap", new SwapCommand(allLists));
factory.Register("Принять", new SwapAcceptCommand(allLists));
factory.Register("Отказаться", new SwapDeclineCommand(allLists));

bot.OnError += OnError;
bot.OnMessage += OnMessage;
bot.OnUpdate += OnUpdate;

Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
Console.ReadLine();

await cts.CancelAsync();

Task OnError(Exception exception, HandleErrorSource source)
{
    Console.WriteLine($"Error: {exception.Message}");
    return Task.CompletedTask;
}

async Task OnMessage(Message msg, UpdateType type)
{
    var searchKey = msg.Text?.Split(' ')[0].Replace(" ", string.Empty);

    if (searchKey == null) return;

    var command = factory.GetCommand(searchKey);

    if (command == null) return;

    await command.Execute(msg, bot);
}

async Task OnUpdate(Update update)
{
    var query = update.CallbackQuery;

    if (query?.Message != null)
    {
        await bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
        await bot.SendMessage(
            query.Message.Chat.Id,
            $"User {query.From.Username} clicked on {query.Data}"
        );
    }
}
