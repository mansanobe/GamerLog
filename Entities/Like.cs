namespace GamerLog.Entities;

public class Like
{
    public Guid LikeId { get; set; }
    
    public Guid ReviewId { get; set; }
    
    public Guid FromUserId { get; set; }
    
    public Guid ToUserId { get; set; }
}