using System.Text.Json;
using GroupBot.Library.Services.Database;

public class AdminLoadService
{
    private readonly IDatabaseService _database;

    public AdminLoadService(IDatabaseService database)
    {
        _database = database;
    }

    public async Task LoadConfig()
    {
        const string filePath = "admins.json";

        if (!File.Exists(filePath))
        {
            Console.WriteLine("admins.json does not exist, skipping config loading");
            return;
        }

        var config = File.ReadAllText(filePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var adminIds = JsonSerializer.Deserialize<List<long>>(config, options);

        if (adminIds == null)
        {
            Console.WriteLine($"There are no admins in JSON file: {filePath}");
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
                Console.WriteLine(e);
            }
        }

        Console.WriteLine("Admins Loaded");
    }
}
