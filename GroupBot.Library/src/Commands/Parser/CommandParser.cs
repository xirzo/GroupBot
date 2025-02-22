using GroupBot.Library.Commands.Repository;
using System.Text;

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

        string[] words = SplitWithQuotes(message);

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

    private static string[] SplitWithQuotes(string input)
    {
        var result = new List<string>();
        var currentWord = new StringBuilder();
        var inQuotes = false;
        var quoteChar = '\0';
        var escaped = false;

        for (int i = 0; i < input.Length; i++)
        {
            char c = input[i];

            if (escaped)
            {
                currentWord.Append(c);
                escaped = false;
                continue;
            }

            if (c == '\\')
            {
                escaped = true;
                continue;
            }

            if (c == '"' || c == '\'')
            {
                if (inQuotes)
                {
                    if (c == quoteChar)
                    {
                        if (currentWord.Length > 0)
                        {
                            result.Add(currentWord.ToString());
                            currentWord.Clear();
                        }
                        inQuotes = false;
                        quoteChar = '\0';
                    }
                    else
                    {
                        currentWord.Append(c);
                    }
                }
                else
                {
                    if (currentWord.Length > 0)
                    {
                        result.Add(currentWord.ToString());
                        currentWord.Clear();
                    }
                    inQuotes = true;
                    quoteChar = c;
                }
                continue;
            }

            if (!inQuotes && char.IsWhiteSpace(c))
            {
                if (currentWord.Length > 0)
                {
                    result.Add(currentWord.ToString());
                    currentWord.Clear();
                }
                continue;
            }

            currentWord.Append(c);
        }

        if (currentWord.Length > 0)
        {
            result.Add(currentWord.ToString());
        }

        if (inQuotes)
        {
            throw new ArgumentException("Unmatched quotes in input string");
        }

        return result.ToArray();
    }
}
