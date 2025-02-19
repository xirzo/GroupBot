namespace GroupBot.Library.Models;
public class User
{
    public long Id { get; set; }
    public long TelegramId { get; set; }
    public required string FullName { get; set; }
    public DateTime CreatedAt { get; set; }

    public virtual ICollection<ListMember> ListMemberships { get; set; } = new List<ListMember>();
}
