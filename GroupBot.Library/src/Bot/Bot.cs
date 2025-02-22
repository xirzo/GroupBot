using GroupBot.Library.Commands.Parser;
using GroupBot.Library.Commands.Repository;
using GroupBot.Library.Services.Command;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;
using GroupBot.Library.Services.Telegram;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace GroupBot.Library.Bot;

public class Bot
{
    public async Task Start()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((services) =>
            {
                services.AddSingleton<ICommandService, CommandService>();
                services.AddSingleton<IDatabaseService, DatabaseService>();
                services.AddSingleton<ITelegramService, TelegramService>();
                services.AddSingleton<IRequestService, RequestService>();

                services.AddSingleton<CommandRepository>();
                services.AddSingleton<CommandParser>();

                var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

                if (string.IsNullOrEmpty(botToken))
                {
                    throw new ArgumentNullException(nameof(botToken), "Bot token is missing");
                }

                services.AddSingleton(new TelegramBotClient(botToken));
            })
            .Build();

        var databaseService = host.Services.GetRequiredService<IDatabaseService>();
        databaseService.Initialize();

        Console.WriteLine("Database initialized");

        var commandService = host.Services.GetRequiredService<ICommandService>();
        commandService.RegisterCommands();

        Console.WriteLine("Commands registered");

        var telegramService = host.Services.GetRequiredService<ITelegramService>();
        await telegramService.StartBot();

        await host.RunAsync();
    }
}
