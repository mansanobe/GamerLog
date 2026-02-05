using GamerLog.Data.DTO;
using GamerLog.Entities;
using GamerLog.Repository.Abstraction;

namespace GamerLog.Services;

public class ReviewService : IReviewService
{
    public IReviewRepository ReviewRepository { get; set; }
    int decayRate = 2;

    public ReviewService(IReviewRepository reviewRepository)
    {
        ReviewRepository = reviewRepository;
    }

    public Review CreateReview(ReviewDto review)
    {
        Review newReview = review.ToEntity();
        newReview.Rating = review.Rating;
        ReviewRepository.Add(newReview);
        return newReview;
    }

    public void RateReview(long likes, Guid reviewId)
    {
        var daysSinceReview = (DateTime.UtcNow - ReviewRepository.GetReviewById(reviewId).LastModified).Days;
        var divisor = Math.Pow(daysSinceReview, decayRate);
        var reviewScore = likes / divisor;
        ReviewRepository.SetReviewScoreById(reviewId, (decimal)reviewScore);
    }
}