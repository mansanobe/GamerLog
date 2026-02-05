using GamerLog.Data.DTO;
using GamerLog.Entities;

namespace GamerLog.Services;

public interface IReviewService
{
    public Review CreateReview(ReviewDto review);

    public void RateReview(long likes, Guid reviewId);
}