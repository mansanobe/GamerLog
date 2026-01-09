using System.Text.Json.Serialization;

namespace GamerLog.Entities;

public class Game
{
    [property:JsonPropertyName("name")]public Guid GameId { get; set; }
    public GameType Type { get; set; }
    
    public enum GameType
    {
        Digital,
        Board,
        Card
    }
}