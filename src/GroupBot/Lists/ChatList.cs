using GroupBot.Parser;

namespace GroupBot.Lists;

public class ChatList
{
    public string Name { get; }
    public long Id { get; }
    private readonly Database.Database _db;

    public ChatList(string name, long id, Database.Database db)
    {
        Name = name;
        Id = id;
        _db = db;
    }

    public async Task Add(Participant participant)
    {
        await _db.CreateUser(participant.Id, participant.Name);
        await _db.TryAddUserToList(participant.Id, Id);
    }

    public async void Swap(long userDbId, long targetDbId)
    {
        try
        {
            await _db.SwapUsersInListAsync(Id, userDbId, targetDbId);
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e}");
        }
    }
}