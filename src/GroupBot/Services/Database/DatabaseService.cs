using GroupBot.Database;
using GroupBot.Parser;
using Microsoft.Extensions.Configuration;

namespace GroupBot.Services.Database;

public class DatabaseService : IDatabaseService
{
    private readonly DatabaseHelper _databaseHelper;
    private readonly IConfiguration _config;

    public DatabaseService(DatabaseHelper databaseHelper, IConfiguration config)
    {
        _databaseHelper = databaseHelper;
        _config = config;
    }

    public void InitializeDatabase()
    {
        var jsonFilePath = _config.GetSection("Participants")["Path"];
        if (string.IsNullOrEmpty(jsonFilePath))
            throw new ArgumentException("Participants JSON file path environment variable is missing");

        var parser = new ParticipantsParser();
        var participants = parser.Parse(jsonFilePath);
        _databaseHelper.InsertParticipants(participants);
    }
}