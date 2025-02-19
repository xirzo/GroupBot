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
        public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<ICommandService, CommandService>();
            services.AddSingleton<IDatabaseService, DatabaseService>();
            services.AddSingleton<IBotService, BotService>();

            services.AddSingleton<RequestsContainer>();
            services.AddSingleton<CommandFactory>();

            var botToken = configuration.GetSection("Tokens")["BotToken"];

            if (string.IsNullOrEmpty(botToken))
            {
                throw new ArgumentException("Bot token is missing in the configuration.");
            }

            services.AddSingleton(new TelegramBotClient(botToken));

            return services;
        }
    }
}
