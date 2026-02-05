using GamerLog.Data;
using GamerLog.Entities;
using GamerLog.Repository.Abstraction;

namespace GamerLog.Repository;

public class ReviewRepository : IReviewRepository
{
    public GamerLogContext DbContext{ get; set; }

    public ReviewRepository(GamerLogContext context)
    {
        DbContext = context;
    }
    
    public Review Add(Review review)
    {
        DbContext.Reviews.Add(review);
        DbContext.SaveChanges();
        return review;
    }
    
    public long GetReviewCountByGameId(Guid gameId)
    {
        return DbContext.Reviews.LongCount(r => r.GameId == gameId);
    }
    
}