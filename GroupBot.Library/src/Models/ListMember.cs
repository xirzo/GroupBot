namespace GroupBot.Library.Models;

public class ListMember
{
    public long Id { get; set; }
    public long ListId { get; set; }
    public long UserId { get; set; }
    public int Position { get; set; }
    public DateTime InsertedAt { get; set; }

    public virtual ChatList List { get; set; } = null!;
    public virtual User User { get; set; } = null!;
}
