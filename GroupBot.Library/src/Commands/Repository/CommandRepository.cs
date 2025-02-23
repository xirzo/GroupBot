namespace GroupBot.Library.Commands.Repository;

public class CommandRepository
{
    private readonly Dictionary<string, ICommand> _commands = new();

    public void Register(ICommand command)
    {
        _commands[command.GetString()] = command;
    }

    public ICommand? GetCommand(string text)
    {
        return _commands.GetValueOrDefault(text);
    }
}
