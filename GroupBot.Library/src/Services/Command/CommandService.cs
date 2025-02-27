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
        _repository.Register(new AddListCommand(_database, _logger));
        _repository.Register(new ToEndCommand(_database, _logger));
        _repository.Register(new ListCommand(_database, _logger));
        _repository.Register(new ListsCommand(_database, _logger));
        _repository.Register(new RemoveListCommand(_database, _logger));
        _repository.Register(new SwapCommand(_requestService, _database, _logger));
        _repository.Register(new SwapAcceptCommand(_requestService, _database, _logger));
        _repository.Register(new SwapDeclineCommand(_requestService, _logger));
        _repository.Register(new SiftCommand(_database, _logger));
        _repository.Register(new HelpCommand(_logger));
        _repository.Register(new AddAdminCommand(_database, _logger));
        _repository.Register(new StartCommand(_database, _logger));

        _logger.Info("Command Service initialized");
    }
}
