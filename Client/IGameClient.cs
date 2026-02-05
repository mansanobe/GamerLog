using GamerLog.Entities;

namespace GamerLog.Client;

public interface IGameClient
{
    public Task<List<Game>> GetGames(int dbGameCount, int lastKnownId);
    
    public Task<List<Genre>> GetGenres(int dbGenresCount);
}