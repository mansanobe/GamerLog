using GamerLog.Client;
using GamerLog.Data;
using GamerLog.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamerLog.Services;

public class GameSyncService
{
    private ILogger<GameSyncService> _logger;
    private readonly IGameClient _gameClient;
    private readonly GamerLogContext _context;
    
    public GameSyncService(IGameClient gameClient, GamerLogContext context, ILogger<GameSyncService> logger)
    {
        _gameClient = gameClient;
        _context = context;
        _logger = logger;
    }

    public async Task SyncGamesFromApiAsync()
    {
        var dbGameCount = await _context.Games.OfType<Game>().CountAsync();
        _logger.LogInformation("Current digital game count in DB: {DbGameCount}", dbGameCount);
        var lastKnownId = await _context.Games.OfType<Game>().MaxAsync(g => (int?)g.ExternalIgdbId) ?? 0;
        
        var gamesFromApi = await _gameClient.GetGames(dbGameCount, lastKnownId);
        
        if (!gamesFromApi.Any() || gamesFromApi == null)
            return;
        
        var apiIds = gamesFromApi.Select(g => g.ExternalIgdbId).Distinct().ToList();
        if (!apiIds.Any())
            return;
        await _context.Games.AddRangeAsync(gamesFromApi);
        await _context.SaveChangesAsync();
    }
    
    public async Task SyncGenresFromApiAsync()
    {
        var dbGenresCount = await _context.Genres.CountAsync();
        _logger.LogInformation("Current genre count in DB: {DbGenresCount}", dbGenresCount);
        
        var genresFromApi = await _gameClient.GetGenres(dbGenresCount);
        if (!genresFromApi.Any() || genresFromApi == null)
            return;
        await _context.Genres.AddRangeAsync(genresFromApi);
        await _context.SaveChangesAsync();
    }
}