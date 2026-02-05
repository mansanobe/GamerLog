namespace GamerLog.Entities;

public class Log
{
    public Guid Id { get; set; }
    public Guid GameId { get; set; }
    public string UserId { get; set; }
    public Guid PlatformId { get; set; }

    public DateTime PlayDate { get; set; }
    
    public int DurationMinutes { get; set; } 
    
    public string? Comment { get; set; } 
}