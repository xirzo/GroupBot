using System.Text.Json;

namespace GroupBot.Parser;

public class ParticipantsParser
{
    public List<Participant> Parse(string jsonFilePath)
    {
        if (!File.Exists(jsonFilePath))
            throw new FileNotFoundException($"JSON file not found: {jsonFilePath}");

        var json = File.ReadAllText(jsonFilePath);
        var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        return JsonSerializer.Deserialize<List<Participant>>(json, options) ?? new List<Participant>();
    }
}