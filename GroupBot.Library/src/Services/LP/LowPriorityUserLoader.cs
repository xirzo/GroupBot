using System.Text.Json;
using GroupBot.Library.Logging;
using GroupBot.Library.Services.Database;

namespace GroupBot.Library.Services.LP;

public class LowPriorityUserLoader
{
    private readonly IDatabaseService _database;
    private readonly ILogger _logger;

    public LowPriorityUserLoader(IDatabaseService database, ILogger logger)
    {
        _database = database;
        _logger = logger;
    }

    public async Task Load()
    {
        const string filePath = "lp_users.json";

        if (!File.Exists(filePath))
        {
            _logger.Info("lp_users.json does not exist, skipping config loading");
            return;
        }

        var config = await File.ReadAllTextAsync(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var lpUsersIds = JsonSerializer.Deserialize<List<long>>(config, options);

        if (lpUsersIds == null)
        {
            Console.WriteLine($"There are no admins in JSON file: {filePath}");
            return;
        }

        foreach (var lpUserId in lpUsersIds)
        {
            try
            {
                await _database.AddLowPriorityUser(lpUserId);
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        _logger.Info("Low priority users loaded");
    }
}