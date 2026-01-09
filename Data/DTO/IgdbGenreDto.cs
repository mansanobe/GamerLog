using System.Text.Json.Serialization;
using GamerLog.Entities;

namespace GamerLog.Data.DTO;

public class IgdbGenreDto
{
    [property:JsonPropertyName("id")]public int Id { get; set; } 
    [property:JsonPropertyName("name")]public string Name { get; set; } 
    
    public Genre ToEntity()
    {
        return new Genre
        {
            Id = this.Id,
            Name = this.Name
        };
    }
}