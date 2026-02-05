namespace GamerLog.Services;

public interface ILikeService
{
    public long GetLikeCountByReviewId(Guid reviewId);
}