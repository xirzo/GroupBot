using System.Text.Json.Serialization;

namespace GroupBot.Library.Models;

public record struct Participant
{
  [JsonPropertyName("id")] public long Id { get; init; }
  [JsonPropertyName("name")] public string Name { get; init; }
  public long Position { get; set; }
}
