using GamerLog.Entities;
using GamerLog.Repository.Abstraction;

namespace GamerLog.Services;

public class GameService : IGameService
{
    IGameRepository _gameRepository;

    public GameService(IGameRepository gameRepository, IReviewService reviewService)
    {
        _gameRepository = gameRepository;
    }
    
    public async Task SumNewRating(long rating, Guid gameId)
    {
        _gameRepository.SumNewRatingById(rating, gameId);
    }

    public bool ExistsById(Guid gameId, out Game game)
    {
        return _gameRepository.ExistsById(gameId, out game);
    }
}