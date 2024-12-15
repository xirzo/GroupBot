using System.Text.Json.Serialization;

namespace GroupBot.Parser;

public record struct Participant
{
  [JsonPropertyName("id")] public long Id { get; set; }
  [JsonPropertyName("name")] public string Name { get; set; }
  public long Position { get; set; }
}
