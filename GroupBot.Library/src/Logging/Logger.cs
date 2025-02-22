using System.Text;

namespace GroupBot.Library.Logging;

public class Logger : ILogger
{
    private readonly LoggerConfiguration _configuration;
    private readonly object _lockObject = new object();
    private readonly string _logFilePath;


    public Logger(LoggerConfiguration? configuration = null)
    {
        _configuration = configuration ?? new LoggerConfiguration();

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd_HH-mm-ss");
        string fileName = $"{_configuration.LogFilePrefix}_{timestamp}.log";

        if (string.IsNullOrWhiteSpace(_configuration.LogFileDirectory))
        {
            _configuration.LogFileDirectory = "logs";
        }

        _logFilePath = Path.Combine(_configuration.LogFileDirectory, fileName);

        if (_configuration.EnableFileLogging)
        {
            try
            {
                string fullPath = Path.GetFullPath(_logFilePath);
                string? directoryPath = Path.GetDirectoryName(fullPath);

                if (string.IsNullOrWhiteSpace(directoryPath))
                {
                    throw new InvalidOperationException("Invalid log directory path");
                }

                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }
            }
            catch (Exception ex)
            {
                _configuration.EnableFileLogging = false;

                if (_configuration.EnableConsoleLogging)
                {
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine($"Failed to setup log directory: {ex.Message}");
                    Console.WriteLine("File logging has been disabled.");
                    Console.ResetColor();
                }
            }
        }
    }

    public void Error(string message)
    {
        LogMessage("ERROR", message);
    }

    public void Error(string message, Exception exception)
    {
        string fullMessage = FormatExceptionMessage(message, exception);
        LogMessage("ERROR", fullMessage);
    }

    public void Error(Exception exception)
    {
        string message = FormatExceptionMessage("An error occurred", exception);
        LogMessage("ERROR", message);
    }

    public void Info(string message)
    {
        LogMessage("INFO", message ?? "null");
    }

    public void Warn(string message)
    {
        LogMessage("WARN", message ?? "null");
    }

    public void Warn(string message, Exception exception)
    {
        string fullMessage = FormatExceptionMessage(message, exception);
        LogMessage("WARN", fullMessage);
    }

    private string FormatExceptionMessage(string message, Exception exception)
    {
        if (exception == null)
            return message;

        var sb = new StringBuilder();
        sb.AppendLine(message);
        sb.AppendLine("Exception Details:");
        sb.AppendLine($"Type: {exception.GetType().FullName}");
        sb.AppendLine($"Message: {exception.Message}");

        if (exception.StackTrace != null)
        {
            sb.AppendLine("Stack Trace:");
            sb.AppendLine(exception.StackTrace);
        }

        var currentException = exception.InnerException;
        while (currentException != null)
        {
            sb.AppendLine("\nInner Exception:");
            sb.AppendLine($"Type: {currentException.GetType().FullName}");
            sb.AppendLine($"Message: {currentException.Message}");

            if (currentException.StackTrace != null)
            {
                sb.AppendLine("Stack Trace:");
                sb.AppendLine(currentException.StackTrace);
            }

            currentException = currentException.InnerException;
        }

        return sb.ToString();
    }

    private void LogMessage(string level, string message)
    {
        if (string.IsNullOrWhiteSpace(level))
        {
            level = "UNKNOWN";
        }

        string timestamp = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss");
        var sb = new StringBuilder();

        foreach (string line in message.Split(new[] { Environment.NewLine }, StringSplitOptions.None))
        {
            if (sb.Length > 0) sb.AppendLine();
            sb.Append($"[{timestamp}] [{level}] {line}");
        }

        string fullLogMessage = sb.ToString();

        lock (_lockObject)
        {
            if (_configuration.EnableConsoleLogging)
            {
                ConsoleColor originalColor = Console.ForegroundColor;
                Console.ForegroundColor = GetColorForMessage(level);
                Console.WriteLine(fullLogMessage);
                Console.ForegroundColor = originalColor;
            }

            if (_configuration.EnableFileLogging)
            {
                try
                {
                    File.AppendAllText(_logFilePath, fullLogMessage + Environment.NewLine);
                }
                catch (Exception ex)
                {
                    if (_configuration.EnableConsoleLogging)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"Failed to write to log file: {ex.Message}");
                        Console.ResetColor();
                    }
                }
            }
        }
    }

    private ConsoleColor GetColorForMessage(string level)
    {
        return level switch
        {
            "ERROR" => ConsoleColor.Red,
            "WARN" => ConsoleColor.Yellow,
            "INFO" => ConsoleColor.Green,
            _ => ConsoleColor.White
        };
    }
}
