namespace GroupBot.Library.Logging;

public interface ILogger
{
    void Error(string message);
    void Error(string message, Exception exception);
    void Error(Exception exception);
    void Info(string message);
    void Warn(string message);
    void Warn(string message, Exception exception);
}
