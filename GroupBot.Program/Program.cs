using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using GroupBot.Extensions;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices((hostContext, services) =>
    {
        services.ConfigureServices(hostContext.Configuration);
    })
    .Build();

var databaseService = host.Services.GetRequiredService<GroupBot.Services.Database.IDatabaseService>();
databaseService.InitializeDatabase();

var commandService = host.Services.GetRequiredService<GroupBot.Services.Command.ICommandService>();
commandService.RegisterCommands();

var botService = host.Services.GetRequiredService<GroupBot.Services.Bot.IBotService>();
await botService.StartBot();

Console.WriteLine("Bot is running... Press any key to terminate");
Console.ReadLine();

await host.RunAsync();
