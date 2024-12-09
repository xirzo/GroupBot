using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot;

internal static class Program
{
    private static TelegramBotClient Bot { get; set; }

    public static async Task Main(string[] args)
    {
        IConfiguration config = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", false, true)
            .Build();

        var botToken = config.GetSection("BotConfiguration")["BotToken"];

        if (string.IsNullOrEmpty(botToken)) throw new ArgumentException("Bot token environment variable is missing");

        using var cts = new CancellationTokenSource();
        Bot = new TelegramBotClient(botToken, cancellationToken: cts.Token);
        var me = await Bot.GetMe();
        Bot.OnError += OnError;
        Bot.OnMessage += OnMessage;
        Bot.OnUpdate += OnUpdate;

        Console.WriteLine($"@{me.Username} is running... Press Enter to terminate");
        Console.ReadLine();
        await cts.CancelAsync();
    }

    private static Task OnError(Exception exception, HandleErrorSource source)
    {
        Console.WriteLine(exception); // just dump the exception to the console
        return Task.CompletedTask;
    }

    private static async Task OnMessage(Message msg, UpdateType type)
    {
        if (msg.Text == "/start")
            await Bot.SendMessage(msg.Chat, "Welcome! Pick one direction",
                replyMarkup: new InlineKeyboardMarkup().AddButtons("Left", "Right"));
    }

    private static async Task OnUpdate(Update update)
    {
        if (update is { CallbackQuery: { } query }) // non-null CallbackQuery
        {
            await Bot.AnswerCallbackQuery(query.Id, $"You picked {query.Data}");
            await Bot.SendMessage(query.Message!.Chat, $"User {query.From} clicked on {query.Data}");
        }
    }
}