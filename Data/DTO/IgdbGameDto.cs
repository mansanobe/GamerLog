using System.Text.Json.Serialization;
using GamerLog.Entities;

namespace GamerLog.Data.DTO;

public class IgdbGameDto
{
    [JsonPropertyName("id")]
    public int IgdbId { get; set; }

    [JsonPropertyName("name")]
    public string Name { get; set; }

    [JsonPropertyName("first_release_date")]
    public long ReleaseDateUnix { get; set; }
    
    [JsonPropertyName("genres")]
    public List<IgdbGenreDto>? Genres { get; set; }
    
    [JsonPropertyName("cover")]
    public IgdbCoverDto? Cover { get; set; }
    
    public Game ToEntity()
    {
        return new Game
        {
            Title = Name,
            ExternalIgdbId = IgdbId,
            ReleaseDateTimestamp = ReleaseDateUnix,
            GenresId = Genres?.ConvertAll(g =>
            {
                return g.Id;
            }),
            CoverUrl = Cover?.Url
        };
    }
}