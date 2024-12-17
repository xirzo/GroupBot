using GroupBot.Commands;
using GroupBot.Commands.Abstract;
using GroupBot.Requests;
using GroupBot.Services.Database;

namespace GroupBot.Services.Command;
public class CommandService : ICommandService
{
  private readonly CommandFactory _factory;
  private readonly IDatabaseService _database;
  private readonly RequestsContainer _requestsContainer;

  public CommandService(CommandFactory factory, IDatabaseService databaseService, RequestsContainer requestsContainer)
  {
    _factory = factory;
    _database = databaseService;
    _requestsContainer = requestsContainer;
  }

  public void RegisterCommands()
  {
    _factory.Register("/start", new StartCommand());
    _factory.Register("/addlist", new AddListCommand(_database));
    _factory.Register("/toend", new ToEndCommand(_database));
    _factory.Register("/list", new ListCommand(_database));
    _factory.Register("/lists", new ListsCommand(_database));
    _factory.Register("/swap", new SwapCommand(_requestsContainer, _database));
    _factory.Register("Принять", new SwapAcceptCommand(_requestsContainer, _database));
    _factory.Register("Отказаться", new SwapDeclineCommand(_requestsContainer));
  }

}
