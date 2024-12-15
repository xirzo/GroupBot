namespace GroupBot.Commands.Abstract;

public class CommandFactory
{
  private readonly Dictionary<string, ICommand> _commands = new();

  public void Register(string commandKeyword, ICommand command)
  {
    _commands[commandKeyword] = command;
  }

  public ICommand? GetCommand(string text)
  {
    return _commands.GetValueOrDefault(text);
  }
}
