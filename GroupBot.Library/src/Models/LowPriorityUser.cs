using System.ComponentModel.DataAnnotations.Schema;

namespace GroupBot.Library.Models;

public class LowPriorityUser
{
    public long Id { get; set; }
    
    public long UserId { get; set; }
    
    [ForeignKey("UserId")]
    public virtual User User { get; set; }
}
