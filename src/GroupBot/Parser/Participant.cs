using System.Text.Json.Serialization;

namespace GroupBot.Shared;

public record struct Participant
{
    [JsonPropertyName("id")] public long Id { get; set; }
    [JsonPropertyName("name")] public string Name { get; set; }
}