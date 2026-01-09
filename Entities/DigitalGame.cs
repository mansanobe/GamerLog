using System.Text.Json.Serialization;

namespace GamerLog.Entities;

public class DigitalGame : Game
{
    
    public string? Title { get; set; }

    public long? ReleaseDateTimestamp { get; set; }
    
    public  int? ExternalIgdbId { get; set; }
    
    public string? CoverUrl { get; set; }
    
    public List<int>? GenresId { get; set; }
}