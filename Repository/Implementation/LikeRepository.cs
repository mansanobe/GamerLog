using GamerLog.Data;
using GamerLog.Repository.Abstraction;

namespace GamerLog.Repository;

public class LikeRepository : ILikeRepository
{
    GamerLogContext context;
    
    public LikeRepository(GamerLogContext context)
    {
        this.context = context;
    }
    public long GetLikeCountByReviewId(Guid reviewId)
    {
        return context.Likes.Where(l => l.ReviewId == reviewId).Count();
    }
}