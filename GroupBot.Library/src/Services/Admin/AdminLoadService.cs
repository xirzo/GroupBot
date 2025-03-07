using System.Text.Json;
using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;

public class AdminLoadService
{
    private readonly IDatabaseService _database;
    private readonly ILogger _logger;

    public AdminLoadService(IDatabaseService database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task Load()
    {
        const string filePath = "admins.json";

        if (!File.Exists(filePath))
        {
            Console.WriteLine("admins.json does not exist, skipping config loading");
            return;
        }

        var config = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var adminIds = JsonSerializer.Deserialize<List<long>>(config, options);

        if (adminIds == null)
        {
            _logger.Info($"There are no admins in JSON file: {filePath}");
            return;
        }

        foreach (var adminId in adminIds)
        {
            try
            {
                await _database.AddAdmin(adminId);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        _logger.Info("Admins Loaded");
    }
}
