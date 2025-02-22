using GroupBot.Library.Commands;
using GroupBot.Library.Commands.Repository;
using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;
using GroupBot.Library.Services.Request;

namespace GroupBot.Library.Services.Command;

public class CommandService : ICommandService
{
    private readonly IDatabaseService _database;
    private readonly CommandRepository _repository;
    private readonly IRequestService _requestService;
    private readonly ILogger _logger;

    public CommandService(CommandRepository repository, IDatabaseService databaseService, IRequestService requestService, ILogger logger)
    {
        _repository = repository;
        _database = databaseService;
        _requestService = requestService;
        _logger = logger;
    }

    public void RegisterCommands()
    {
        _repository.Register("/start", new StartCommand(_database));
        _repository.Register("/addlist", new AddListCommand(_database));
        _repository.Register("/toend", new ToEndCommand(_database));
        _repository.Register("/list", new ListCommand(_database));
        _repository.Register("/lists", new ListsCommand(_database));
        _repository.Register("/removelist", new RemoveListCommand(_database, _logger));
        _repository.Register("/swap", new SwapCommand(_requestService, _database));
        _repository.Register("Принять", new SwapAcceptCommand(_requestService, _database));
        _repository.Register("Отказаться", new SwapDeclineCommand(_requestService));
        _repository.Register("/sift", new SiftCommand(_database, _logger));
        _repository.Register("/help", new HelpCommand());
        _repository.Register("/addadmin", new AddAdminCommand(_database, _logger));

        _logger.Info("Command Service initialized");
    }
}
