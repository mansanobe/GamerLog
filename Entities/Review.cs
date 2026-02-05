namespace GamerLog.Entities;

public class Review
{
    public Guid GameId { get; set; }
    public Guid ReviewId { get; set; }
    public int Rating { get; set; }
    public List<string> Pros { get; set; } = new();
    public List<string> Cons { get; set; } = new();
    
    public string Content { get; set; } = string.Empty;
    public bool IsVerifiedCritic { get; set; }
    public bool IsSpoiler { get; set; }
    public DateTime LastModified { get; set; }

    public decimal ReviewScore { get; set; }
    
}