namespace GroupBot.Library.Logging
{
    public static class LogMessages
    {
        public static string CommandStarted(string commandName, string username, string? target = null, long chatId = 0) =>
            $"Command '{commandName}' initiated by @{username}" +
            (target != null ? $" for user '{target}'" : "") +
            $" in chat {chatId}";

        public static string CommandCompleted(string commandName, string username, string? target) =>
            $"Command '{commandName}' successfully completed by @{username}" +
            (target != null ? $" for user '{target}'" : "");

        public static string AccessDenied(string commandName, string username) =>
            $"Access denied: User @{username} attempted to execute '{commandName}' command without admin privileges";

        public static string DatabaseOperationSuccess(string operation, string target, string username, long id = 0) =>
            $"{operation} successful: User '{target}'" +
            (id != 0 ? $" (ID: {id})" : "") +
            $" processed by @{username}";

        public static string DatabaseOperationFailed(string operation, string target, string username, Exception? ex = null) =>
            $"{operation} failed for '{target}' requested by @{username}" +
            (ex != null ? $": {ex.Message}" : "");

        public static string NotFound(string target, string username) =>
            $"Not found: Unable to find target: '{target}' requested by @{username}";

        public static string ErrorOccurred(string operation, string details, Exception? ex = null) =>
            $"Error during {operation}: {details}" +
            (ex != null ? $"\nException: {ex.Message}" : "");
    }
}
