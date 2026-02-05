using System.Text.Json.Serialization;

namespace GamerLog.Entities;

public class Game
{
    public Guid GameId { get; set; }
    public GameType Type { get; set; }
    public string Title { get; set; }
    public long? ReleaseDateTimestamp { get; set; }
    public  int ExternalIgdbId { get; set; }
    public string? CoverUrl { get; set; }
    public List<int>? GenresId { get; set; }
    
    public long? RatingSum { get; set; }
    
    public long? RatingCount { get; set; }
    
    public enum GameType
    {
        Digital,
        Board,
        Card
    }
}