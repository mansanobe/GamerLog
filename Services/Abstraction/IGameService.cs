using GamerLog.Entities;

namespace GamerLog.Services;

public interface IGameService
{
    public Task SumNewRating(long rating, Guid gameId);
    
    public bool ExistsById(Guid gameId, out Game game);
}