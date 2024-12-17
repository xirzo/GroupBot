using GroupBot.Commands.Abstract;
using GroupBot.Database;
using GroupBot.Requests;
using GroupBot.Services.Bot;
using GroupBot.Services.Command;
using GroupBot.Services.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Telegram.Bot;

namespace GroupBot;

class Program
{
  private static async Task Main(string[] args)
  {
    var host = CreateHostBuilder(args).Build();

    var botService = host.Services.GetRequiredService<IBotService>();
    var commandService = host.Services.GetRequiredService<ICommandService>();
    var databaseService = host.Services.GetRequiredService<IDatabaseService>();

    databaseService.InitializeDatabase();
    commandService.RegisterCommands();

    await botService.StartBot();

    Console.WriteLine("Bot is running... Press Enter to terminate");
    Console.ReadLine();

    await host.RunAsync();
  }

  private static IHostBuilder CreateHostBuilder(string[] args) =>
      Host.CreateDefaultBuilder(args)
          .ConfigureServices((hostContext, services) =>
          {

            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IBotService, BotService>();

            services.AddSingleton<RequestsContainer>();
            services.AddSingleton<CommandFactory>();

            services.AddSingleton<DatabaseHelper>(provider =>
              {
                var config = provider.GetRequiredService<IConfiguration>();
                var dbPath = config.GetSection("Database")["Path"];

                if (string.IsNullOrEmpty(dbPath))
                  throw new ArgumentException("DB Path environment variable is missing");

                return new DatabaseHelper(dbPath);
              });

            services.AddSingleton<TelegramBotClient>(provider =>
              {
                var config = provider.GetRequiredService<IConfiguration>();
                var botToken = config.GetSection("Tokens")["BotToken"];

                if (string.IsNullOrEmpty(botToken))
                  throw new ArgumentException("Bot token environment variable is missing");

                return new TelegramBotClient(botToken);
              });
          });
}
