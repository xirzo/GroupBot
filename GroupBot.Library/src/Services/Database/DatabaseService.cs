using System.Text.Json;
using GroupBot.Library.Models;
using Microsoft.Extensions.Configuration;

namespace GroupBot.Library.Services.Database;

public class DatabaseService : IDatabaseService
{
    private readonly IConfiguration _config;
    private readonly DatabaseHelper _databaseHelper;
    private List<Participant> _admins;
    private List<ChatList> _lists;

    public DatabaseService(IConfiguration config)
    {
        var dbPath = config.GetSection("Database")["Path"];

        if (string.IsNullOrEmpty(dbPath))
        {
            throw new ArgumentException("Database path is missing in the configuration file");
        }

        _databaseHelper = new DatabaseHelper(dbPath);
        _admins = [];
        _lists = [];
        _config = config;
    }

    public void InitializeDatabase()
    {
        var jsonFilePath = _config.GetSection("Participants")["Path"];

        if (string.IsNullOrEmpty(jsonFilePath))
            throw new ArgumentException("Participants JSON file path environment variable is missing");

        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"JSON file not found: {jsonFilePath}");

        var json = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        var participants = JsonSerializer.Deserialize<List<Participant>>(json, options);

        if (participants == null)
        {
            throw new ArgumentException($"There are no participants in JSON file: {jsonFilePath}");
        }

        _databaseHelper.InsertParticipants(participants);
    }

    public async Task<List<ChatList>> GetAllLists()
    {
        if (_lists.Count > 0) return _lists;

        return await _databaseHelper.GetAllLists();
    }

    public async Task<long> CreateListAndShuffle(string listName)
    {
        var id = await _databaseHelper.CreateListAndShuffle(listName);
        _lists = await _databaseHelper.GetAllLists();
        return id;
    }

    public async Task<List<Participant>> GetAllParticipantsInList(long id)
    {
        return await _databaseHelper.GetAllUsersInList(id);
    }

    public async Task<long> GetParticipantIdByTelegramId(long id)
    {
        return await _databaseHelper.GetParticipantIdByTelegramId(id);
    }

    public async Task MoveUserToEndOfList(long listId, long userId)
    {
        await _databaseHelper.MoveUserToEndOfListAsync(listId, userId);
    }

    public async Task SwapParticipantsInList(long id, long userDbId, long targetDbId)
    {
        await _databaseHelper.SwapParticipantsInList(id, userDbId, targetDbId);
    }

    public async Task<List<Participant>> GetAllAdmins()
    {
        if (_admins.Count > 0) return _admins;

        _admins = await _databaseHelper.GetAllAdmins();
        return _admins;
    }

    public async Task RemoveList(long listId)
    {
        await _databaseHelper.RemoveList(listId);
    }
}