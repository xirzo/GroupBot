using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Requests;
using GroupBot.Library.Services.Bot;
using GroupBot.Library.Services.Command;
using GroupBot.Library.Services.Database;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;

namespace GroupBot.Library.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static void ConfigureServices(this IServiceCollection services)
        {
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IBotService, BotService>();

            services.AddSingleton<RequestsContainer>();
            services.AddSingleton<CommandFactory>();

            var botToken = Environment.GetEnvironmentVariable("BOT_TOKEN");

            if (string.IsNullOrEmpty(botToken))
            {
                throw new ArgumentException("Bot token is missing");
            }

            services.AddSingleton(new TelegramBotClient(botToken));
        }
    }
}
