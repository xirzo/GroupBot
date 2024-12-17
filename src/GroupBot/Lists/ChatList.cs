using GroupBot.Services.Database;

namespace GroupBot.Lists;

public class ChatList
{
  public string Name { get; }
  public long Id { get; }

  public ChatList(string name, long id)
  {
    Name = name;
    Id = id;
  }

  public async void Swap(long userDbId, long targetDbId, IDatabaseService db)
  {
    try
    {
      await db.SwapParticipantsInList(Id, userDbId, targetDbId);
    }

    catch (Exception e)
    {
      Console.WriteLine($"Error: {e}");
    }
  }
}
