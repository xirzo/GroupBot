using GroupBot.Library.Commands.Repository;

namespace GroupBot.Library.Commands.Parser;

public class CommandParser
{
    private readonly CommandRepository _factory;

    public CommandParser(CommandRepository factory)
    {
        _factory = factory;
    }

    public CommandParseResult Parse(string message)
    {
        if (string.IsNullOrEmpty(message))
        {
            return CommandParseResult.Error("❌ Сообщение пустое");
        }

        string[] words = message.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        if (words.Length == 0)
        {
            return CommandParseResult.Error("❌ Не задана команда");
        }

        string commandKey = words[0];

        ICommand? command = _factory.GetCommand(commandKey);

        if (command is null)
        {
            return CommandParseResult.Error($"❌ Команды {commandKey} не существует");
        }

        string[] parameters = words.Skip(1).ToArray();

        if (command.NumberOfArguments != parameters.Length)
        {
            return CommandParseResult.Error(
                $"❌ Неверное количество аргументов для команды {commandKey}. " +
                $"Ожидается: {command.NumberOfArguments}, Получено: {parameters.Length}");
        }

        return new CommandParseResult(
            command: command,
            success: true,
            hasParameters: parameters.Length > 0,
            parameters: parameters,
            errorMessage: string.Empty);
    }
}
