using GamerLog.Data;
using GamerLog.Entities;
using GamerLog.Repository.Abstraction;

namespace GamerLog.Repository;

public class GameRepository : IGameRepository
{
    private GamerLogContext _context;

    public GameRepository(GamerLogContext context)
    {
        _context = context;
    }

    public void SumNewRatingById(long rating, Guid gameId)
    {
        var game = _context.Games.Find(gameId);
        if (game != null)
        {
            game.RatingSum = (game.RatingSum ?? 0) + rating;
            game.RatingCount = (game.RatingCount ?? 0) + 1;
            _context.SaveChanges();
        }
    }

    public bool ExistsById(Guid gameId, out Game game)
    {
        game = _context.Games.Find(gameId);
        return game != null;
    }
}