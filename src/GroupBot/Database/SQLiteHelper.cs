using System.Data.SQLite;

namespace GroupBot.Database;

public class SQLiteHelper
{
    private readonly string _connectionString;

    public SQLiteHelper(string dbPath)
    {
        _connectionString = $"Data Source={dbPath};Version=3;";
    }

    public async Task ExecuteQueryAsync(string query, params SQLiteParameter[] parameters)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        using var command = new SQLiteCommand(query, connection);
        if (parameters != null)
        {
            command.Parameters.AddRange(parameters);
        }

        await command.ExecuteNonQueryAsync();
    }

    public async Task<bool> UserExistsAsync(long telegramId)
    {
        using var connection = new SQLiteConnection(_connectionString);
        await connection.OpenAsync();

        using var command =
            new SQLiteCommand("SELECT COUNT(1) FROM users WHERE telegram_id = @telegram_id", connection);
        command.Parameters.AddWithValue("@telegram_id", telegramId);

        var count = Convert.ToInt32(await command.ExecuteScalarAsync());
        return count > 0;
    }
}