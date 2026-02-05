using GamerLog.Data.DTO;
using GamerLog.Entities;
using GamerLog.Services;
using Microsoft.AspNetCore.Mvc;

namespace GamerLog.Controllers;

[ApiController]
[Route("api/review")]
public class ReviewController
{
    public IReviewService ReviewService { get; set; }
    public IGameService GameService { get; set; }
    public ILogger<ReviewController> Logger { get; set; }
    

    public ReviewController(IReviewService reviewService, IGameService gameService, ILogger<ReviewController> logger)
    {
        ReviewService = reviewService;
        GameService = gameService;
        Logger = logger;
    }
    
    [HttpPost("create")]
    public ActionResult<HttpMessage> CreateReview(ReviewDto reviewDto)
    {
        if(!Validate(reviewDto, out var errors))
        {
            string errorDetails = "";
            foreach (var error in errors)
            {
                errorDetails += error + "\n";
            }
            var errorMessage = new HttpMessage
            {
                Code = "400",
                Message = "Validation Failed: " + errorDetails
            };
            return new ObjectResult(errorMessage) { StatusCode = 400 };
        }
        try
        {
            var result = ReviewService.CreateReview(reviewDto);
            if (result.ReviewId == Guid.Empty)
            {
                var errorMessage = new HttpMessage
                {
                    Code = "500",
                    Message = "Failed to create review."
                };
                
                return new ObjectResult(errorMessage) { StatusCode = 500 };
            }
            GameService.SumNewRating(result.Rating, result.GameId);
            var successMessage = new HttpMessage
            {
                Code = "201",
                Message = "Review created successfully."
            };
            return new ObjectResult(successMessage) { StatusCode = 201 };
        }
        catch (Exception ex)
        {
            var errorMessage = new HttpMessage
            {
                Code = "500",
                Message = $"Internal Error: {ex.Message}"
            };
            return new ObjectResult(errorMessage) { StatusCode = 500 };
        }
    }

    private bool Validate(ReviewDto reviewDto, out List<string> errors)
    {
        errors = new List<string>();
        if (reviewDto.GameId == Guid.Empty)
        {
            Logger.LogError("GameId is required.");
            errors.Add("GameId is required.");
        }
        if(reviewDto.Rating < 0 || reviewDto.Rating > 10)
        {
            Logger.LogError("Rating must be between 0 and 10.");
            errors.Add("Rating must be between 0 and 10.");
        }
        if(string.IsNullOrWhiteSpace(reviewDto.Content))
        {
            Logger.LogError("Content is required.");
            errors.Add("Content is required.");
        }

        if (!GameService.ExistsById(reviewDto.GameId, out Game _))
        {
            Logger.LogError("Game doesn't exist.");
            errors.Add("Game doesn't exist.");
        }
        return errors.Count == 0;
    }
    
}