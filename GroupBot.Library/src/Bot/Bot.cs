using GroupBot.Library.Commands.Parser;
using GroupBot.Library.Commands.Repository;
using GroupBot.Library.Logging;
using GroupBot.Library.Services.Command;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.LP;
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
                services.AddSingleton<AdminLoadService>();
                services.AddSingleton<LowPriorityUserLoader>();

                services.AddSingleton(new LoggerConfiguration
                {
                    EnableConsoleLogging = true,
                    EnableFileLogging = true,
                    LogFileDirectory = "/app/logs",
                    LogFilePrefix = "groupbot"
                });

                services.AddSingleton<CommandRepository>();
                services.AddSingleton<CommandParser>();

                services.AddSingleton<ILogger, Logger>();

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

        var commandService = host.Services.GetRequiredService<ICommandService>();
        commandService.RegisterCommands();

        var adminLoadService = host.Services.GetRequiredService<AdminLoadService>();
        await adminLoadService.Load();
        
        
        var lpUserLoadService = host.Services.GetRequiredService<LowPriorityUserLoader>();
        await lpUserLoadService.Load();

        var telegramService = host.Services.GetRequiredService<ITelegramService>();
        await telegramService.StartBot();

        await host.RunAsync();
    }
}
