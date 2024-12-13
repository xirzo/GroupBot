using System.Text.Json;
using GroupBot.Shared;

namespace GroupBot.Loader;

public class ParticipantsParser
{
    public List<Participant> LoadParticipants(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"JSON file not found: {jsonFilePath}");

        var json = File.ReadAllText(jsonFilePath);
        return JsonSerializer.Deserialize<List<Participant>>(json) ?? new List<Participant>();
    }
}