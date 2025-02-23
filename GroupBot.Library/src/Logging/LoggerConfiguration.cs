namespace GroupBot.Library.Logging;

public class LoggerConfiguration
{
    public bool EnableConsoleLogging { get; set; } = true;
    public bool EnableFileLogging { get; set; } = false;
    private string _logFileDirectory = "/app/logs";
    public string LogFileDirectory
    {
        get => _logFileDirectory;
        set => _logFileDirectory = string.IsNullOrWhiteSpace(value) ? "/app/logs" : value;
    }
    private string _logFilePrefix = "application";
    public string LogFilePrefix
    {
        get => _logFilePrefix;
        set => _logFilePrefix = string.IsNullOrWhiteSpace(value) ? "application" : value;
    }
}

