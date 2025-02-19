using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Requests;
using GroupBot.Library.Services.Bot;
using GroupBot.Library.Services.Command;
using GroupBot.Library.Services.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace GroupBot.Library.Bot;

public class Bot
{
    public async Task Start()
    {
        var host = Host.CreateDefaultBuilder()
            .ConfigureServices((hostContext, services) =>
            {
                services.AddSingleton<ICommandService, CommandService>();
                services.AddSingleton<IDatabaseService, DatabaseService>();
                services.AddSingleton<IBotService, BotService>();

                services.AddSingleton<RequestsContainer>();
                services.AddSingleton<CommandFactory>();

                var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

                if (string.IsNullOrEmpty(botToken)) throw new ArgumentException("Bot token is missing");

                services.AddSingleton(new TelegramBotClient(botToken));
            })
            .Build();

        var databaseService = host.Services.GetRequiredService<IDatabaseService>();
        databaseService.InitializeDatabase();

        var commandService = host.Services.GetRequiredService<ICommandService>();
        commandService.RegisterCommands();

        var botService = host.Services.GetRequiredService<IBotService>();
        await botService.StartBot();

        Console.WriteLine("Bot is running... Press any key to terminate");
        Console.ReadLine();

        await host.RunAsync();
    }
}