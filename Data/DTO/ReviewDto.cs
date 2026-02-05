using System.Text.Json.Serialization;
using GamerLog.Entities;

namespace GamerLog.Data.DTO;

public class ReviewDto : BaseDto
{
    [JsonPropertyName("GameId")]
    public Guid GameId { get; set; }
    
    [JsonPropertyName("Rating")]
    public int Rating { get; set; }
    
    [JsonPropertyName("Pros")]
    public List<string>? Pros { get; set; } = new();
    
    [JsonPropertyName("Cons")]
    public List<string>? Cons { get; set; } = new();
    
    [JsonPropertyName("Content")]
    public string Content { get; set; } = string.Empty;
    
    [JsonPropertyName("IsVerifiedCritic")]
    public bool? IsVerifiedCritic { get; set; }
    
    [JsonPropertyName("IsSpoiler")]
    public bool? IsSpoiler { get; set; }
    
    [JsonPropertyName("LastModified")]
    public DateTime? LastModified { get; set; }
    
    public ReviewDto() { }
    
    public ReviewDto(Review review)
    {
        GameId = review.GameId;
        Rating = review.Rating;
        Pros = review.Pros;
        Cons = review.Cons;
        Content = review.Content;
        IsVerifiedCritic = review.IsVerifiedCritic;
        IsSpoiler = review.IsSpoiler;
        LastModified = review.LastModified;
    }
    
    public Review ToEntity()
    {
        return new Review()
        {
            GameId = GameId,
            Rating = Rating,
            Pros = Pros,
            Cons = Cons,
            Content = Content,
            IsVerifiedCritic = IsVerifiedCritic ?? false,
            IsSpoiler = IsSpoiler ?? false,
            LastModified = LastModified ?? DateTime.UtcNow
        };
    }
    
    
}