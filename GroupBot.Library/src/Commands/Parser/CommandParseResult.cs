using GroupBot.Library.Commands.Abstract;

public record CommandParseResult
{
    private readonly ICommand? _command;

    public ICommand Command => Success
        ? _command!
        : throw new InvalidOperationException("Cannot access Command when Success is false");

    public bool Success { get; }
    public bool HasParameters { get; }
    public string[] Parameters { get; }
    public string ErrorMessage { get; }

    public CommandParseResult(
        ICommand? command,
        bool success,
        bool hasParameters,
        string[] parameters,
        string errorMessage)
    {
        _command = command;
        Success = success;
        HasParameters = hasParameters;
        Parameters = parameters ?? Array.Empty<string>();
        ErrorMessage = errorMessage;

        if (success && command == null)
        {
            throw new ArgumentException("Command cannot be null when Success is true");
        }
    }

    public static CommandParseResult Error(string message) =>
        new(
            command: null,
            success: false,
            hasParameters: false,
            parameters: Array.Empty<string>(),
            errorMessage: message);
}
