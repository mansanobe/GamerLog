namespace GamerLog.Repository.Abstraction;

public interface ILikeRepository
{
    public long GetLikeCountByReviewId(Guid reviewId);
}