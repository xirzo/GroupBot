using System.Text.Json.Serialization;

namespace GroupBot.Library.Models;

public class Participant
{
    [JsonPropertyName("id")]
    public long Id { get; init; }

    [JsonPropertyName("name")]
    public string Name { get; init; } = string.Empty;

    public int Position { get; init; }

    public Participant() { }

    public Participant(long id, string name, int position = 0)
    {
        Id = id;
        Name = name;
        Position = position;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Participant other)
        {
            return false;
        }

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name} (ID: {Id}, Position: {Position})";
    }
}
