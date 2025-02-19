using GroupBot.Library.Commands;
using GroupBot.Library.Commands.Abstract;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;

namespace GroupBot.Library.Services.Command;

public class CommandService : ICommandService
{
    private readonly IDatabaseService _database;
    private readonly CommandFactory _factory;
    private readonly IRequestService _requestService;

    public CommandService(CommandFactory factory, IDatabaseService databaseService, IRequestService requestService)
    {
        _factory = factory;
        _database = databaseService;
        _requestService = requestService;
    }

    public void RegisterCommands()
    {
        _factory.Register("/start", new StartCommand(_database));
        _factory.Register("/addlist", new AddListCommand(_database));
        _factory.Register("/toend", new ToEndCommand(_database));
        _factory.Register("/list", new ListCommand(_database));
        _factory.Register("/lists", new ListsCommand(_database));
        _factory.Register("/removelist", new RemoveListCommand(_database));
        _factory.Register("/swap", new SwapCommand(_requestService, _database));
        _factory.Register("Принять", new SwapAcceptCommand(_requestService, _database));
        _factory.Register("Отказаться", new SwapDeclineCommand(_requestService));
    }
}