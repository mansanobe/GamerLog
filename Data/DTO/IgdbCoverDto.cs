using System.Text.Json.Serialization;

namespace GamerLog.Data.DTO;

public class IgdbCoverDto
{
    [property:JsonPropertyName("id")]
    public int Id { get; set; } 
    
    [property:JsonPropertyName("url")]
    public string Url { get; set; }
}