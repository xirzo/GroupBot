using GroupBot.Library.Extensions;
using GroupBot.Library.Services.Bot;
using GroupBot.Library.Services.Command;
using GroupBot.Library.Services.Database;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.ConfigureServices();
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
