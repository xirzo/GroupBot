using GroupBot.Commands;
using GroupBot.Commands.Abstract;
using GroupBot.Database;
using GroupBot.Lists;
using GroupBot.Parser;
using GroupBot.Requests;
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
    .AddJsonFile("appsettings.json", false, true)
    .Build();

var botToken = config.GetSection("Tokens")["BotToken"];

if (string.IsNullOrEmpty(botToken))
    throw new ArgumentException("Bot token environment variable is missing");

using var cts = new CancellationTokenSource();

bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);

if (bot == null)
    throw new ArgumentException("Bot is null");

var dbPath = config.GetSection("Database")["Path"];

if (string.IsNullOrEmpty(dbPath))
    throw new ArgumentException("DB Path environment variable is missing");

var sqliteHelper = new DatabaseHelper(dbPath);

var jsonFilePath = config.GetSection("Participants")["Path"];

if (string.IsNullOrEmpty(jsonFilePath))
    throw new ArgumentException("Participants JSON file path environment variable is missing");

var parser = new ParticipantsParser();
var participants = parser.Parse(jsonFilePath);
sqliteHelper.InsertParticipants(participants);

var me = await bot.GetMe(cts.Token);

var requestContainer = new RequestsContainer();

factory.Register("/start", new StartCommand());
factory.Register("/addlist", new AddListCommand(sqliteHelper));
factory.Register("/toend", new ToEndCommand(sqliteHelper));
factory.Register("/list", new ListCommand(sqliteHelper));
factory.Register("/lists", new ListsCommand(sqliteHelper));
factory.Register("/swap", new SwapCommand(requestContainer, sqliteHelper));
factory.Register("Принять", new SwapAcceptCommand(requestContainer, sqliteHelper));
factory.Register("Отказаться", new SwapDeclineCommand(requestContainer));

bot.OnError += OnError;
bot.OnMessage += OnMessage;

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