using GroupBot.Commands;
using GroupBot.Commands.Abstract;
using GroupBot.Database;
using GroupBot.Requests;
using Telegram.Bot;
using Telegram.Bot.Types;

namespace GroupBot.Services.Command;
public class CommandService : ICommandService
{
    private readonly CommandFactory _factory;
    private readonly DatabaseHelper _databaseHelper;
    private readonly RequestsContainer _requestsContainer;

    public CommandService(CommandFactory factory, DatabaseHelper databaseHelper, RequestsContainer requestsContainer)
    {
        _factory = factory;
        _databaseHelper = databaseHelper;
        _requestsContainer = requestsContainer;
    }

    public void RegisterCommands()
    {
        _factory.Register("/start", new StartCommand());
        _factory.Register("/addlist", new AddListCommand(_databaseHelper));
        _factory.Register("/toend", new ToEndCommand(_databaseHelper));
        _factory.Register("/list", new ListCommand(_databaseHelper));
        _factory.Register("/lists", new ListsCommand(_databaseHelper));
        _factory.Register("/swap", new SwapCommand(_requestsContainer, _databaseHelper));
        _factory.Register("Принять", new SwapAcceptCommand(_requestsContainer, _databaseHelper));
        _factory.Register("Отказаться", new SwapDeclineCommand(_requestsContainer));
    }

}