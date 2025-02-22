using GroupBot.Library.Commands;
using GroupBot.Library.Commands.Repository;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;

namespace GroupBot.Library.Services.Command;

public class CommandService : ICommandService
{
    private readonly IDatabaseService _database;
    private readonly CommandRepository _repository;
    private readonly IRequestService _requestService;

    public CommandService(CommandRepository repository, IDatabaseService databaseService, IRequestService requestService)
    {
        _repository = repository;
        _database = databaseService;
        _requestService = requestService;
    }

    public void RegisterCommands()
    {
        _repository.Register("/start", new StartCommand(_database));
        _repository.Register("/addlist", new AddListCommand(_database));
        _repository.Register("/toend", new ToEndCommand(_database));
        _repository.Register("/list", new ListCommand(_database));
        _repository.Register("/lists", new ListsCommand(_database));
        _repository.Register("/removelist", new RemoveListCommand(_database));
        _repository.Register("/swap", new SwapCommand(_requestService, _database));
        _repository.Register("Принять", new SwapAcceptCommand(_requestService, _database));
        _repository.Register("Отказаться", new SwapDeclineCommand(_requestService));
        _repository.Register("/sift", new SiftCommand(_database));
        _repository.Register("/help", new HelpCommand());
        _repository.Register("/addadmin", new AddAdminCommand(_database));
    }
}
