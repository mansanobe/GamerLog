using GamerLog.Repository.Abstraction;

namespace GamerLog.Services;

public class LikeService : ILikeService
{
    public ILikeRepository LikeRepository { get; set; }
    
    public LikeService(ILikeRepository likeRepository)
    {
        LikeRepository = likeRepository;
    }
    
    public long GetLikeCountByReviewId(Guid reviewId)
    {
        return LikeRepository.GetLikeCountByReviewId(reviewId);
    }
}