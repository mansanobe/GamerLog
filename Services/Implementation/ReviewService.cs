using GamerLog.Data.DTO;
using GamerLog.Entities;
using GamerLog.Repository.Abstraction;

namespace GamerLog.Services;

public class ReviewService : IReviewService
{
    public IReviewRepository ReviewRepository { get; set; }

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

    public List<string> RateReview(Guid reviewId)
    {
        
    }
}