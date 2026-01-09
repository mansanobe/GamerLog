using GamerLog.Entities;

namespace GamerLog.Client;

public interface IGameClient
{
    public Task<List<DigitalGame>> GetGames();
    
    public Task<List<Genre>> GetGenres();
}