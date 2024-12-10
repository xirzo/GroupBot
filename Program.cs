using Microsoft.Extensions.Configuration;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;

namespace GroupBot;

internal static class Program
{
    private static TelegramBotClient? Bot { get; set; }

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

        var me = await Bot.GetMeAsync(cts.Token);
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
        if (msg.Text == "/start")
        {
            var replyMarkup = new InlineKeyboardMarkup(new[]
            {
                new[]
                {
                    InlineKeyboardButton.WithCallbackData("📝 Открыть списки"),
                    InlineKeyboardButton.WithCallbackData("❌ Закрыть")
                }
            });

            await Bot.SendTextMessageAsync(
                msg.Chat.Id,
                "Привет! Выбери команду.",
                replyMarkup: replyMarkup
            );
        }
    }

    private static async Task OnUpdate(Update update)
    {
        if (update.CallbackQuery != null)
        {
            var query = update.CallbackQuery;
            await Bot.AnswerCallbackQueryAsync(query.Id, $"You picked {query.Data}");
            await Bot.SendTextMessageAsync(
                query.Message.Chat.Id,
                $"User {query.From.Username} clicked on {query.Data}"
            );
        }
    }
}