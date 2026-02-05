using GamerLog.Entities;

namespace GamerLog.Repository.Abstraction;

public interface IGameRepository
{
    public void SumNewRatingById(long rating, Guid gameId);
    
    public bool ExistsById(Guid gameId, out Game game);
}