using GroupBot.Library.Services.Database;

namespace GroupBot.Library.Models;
public class ChatList
{
    public long Id { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ListMember> Members { get; set; } = new List<ListMember>();

    public Task SwapAsync(long userDbId, long targetDbId, IDatabaseService db)
    {
        return db.SwapParticipantsInList(Id, userDbId, targetDbId);
    }
}
