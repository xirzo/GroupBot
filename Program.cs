using GroupBot.Commands;
using GroupBot.Lists;
using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace GroupBot;

internal static class Program
{
    private static readonly List<ChatList> AllLists = new();
    private static readonly CommandFactory Factory = new();
    private static TelegramBotClient Bot { get; set; } = null!;

    public static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var botToken = config.GetSection("BotConfiguration")["BotToken"];

        if (string.IsNullOrEmpty(botToken))
            throw new ArgumentException("Bot token environment variable is missing");

        using var cts = new CancellationTokenSource();
        Bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);

        if (Bot == null)
            throw new ArgumentException("Bot is null");

        var me = await Bot.GetMe(cts.Token);

        Factory.Register("/start", new StartCommand());
        Factory.Register("/addlist", new AddListCommand(AllLists));
        Factory.Register("/addtolist", new AddToListCommand(AllLists));
        Factory.Register("/shufflelist", new ShuffleListCommand(AllLists));
        Factory.Register("/removefromlist", new RemoveFromListCommand(AllLists));
        Factory.Register("/list", new ListCommand(AllLists));
        Factory.Register("/lists", new ListsCommand(AllLists));
        Factory.Register("/openlists", new OpenListsCommand(AllLists));

        Bot.OnError += OnError;
        Bot.OnMessage += OnMessage;
        Bot.OnUpdate += OnUpdate;

        Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
        Console.ReadLine();

        await cts.CancelAsync();
    }

    private static Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine($"Error: {exception.Message}");
        return Task.CompletedTask;
    }

    private static async Task OnMessage(Message msg, UpdateType type)
    {
        var searchKey = msg.Text?.Split(' ')[0].Replace(" ", string.Empty);

        if (searchKey == null) return;

        var command = Factory.GetCommand(searchKey);

        if (command == null) return;

        await command.Execute(msg, Bot);
    }

    private static async Task OnUpdate(Update update)
    {
        var query = update.CallbackQuery;

        if (query?.Message != null)
        {
            await Bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
            await Bot.SendMessage(
                query.Message.Chat.Id,
                $"User {query.From.Username} clicked on {query.Data}"
            );
        }
    }
}