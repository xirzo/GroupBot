using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Telegram.Bot;
using GroupBot.Commands.Abstract;
using GroupBot.Requests;
using GroupBot.Services.Bot;
using GroupBot.Services.Command;
using GroupBot.Services.Database;

namespace GroupBot.Extensions
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
