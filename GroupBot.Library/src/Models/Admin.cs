namespace GroupBot.Library.Models;

public class Admin
{
    public long Id { get; set; }
    public long UserId { get; set; }

    public virtual required User User { get; set; }
}
