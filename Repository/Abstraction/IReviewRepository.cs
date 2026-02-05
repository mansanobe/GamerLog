using GamerLog.Entities;

namespace GamerLog.Repository.Abstraction;

public interface IReviewRepository
{
    public Review Add(Review review);
    
    public long GetReviewCountByGameId(Guid gameId);
    
    public Review GetReviewById(Guid reviewId);
    
    public void SetReviewScoreById(Guid reviewId, decimal score);
}