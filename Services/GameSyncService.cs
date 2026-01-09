using GamerLog.Client;
using GamerLog.Data;
using GamerLog.Entities;
using Microsoft.EntityFrameworkCore;

namespace GamerLog.Services;

public class GameSyncService
{
    private readonly IGameClient _gameClient;
    private readonly GamerLogContext _context;
    
    public GameSyncService(IGameClient gameClient, GamerLogContext context)
    {
        _gameClient = gameClient;
        _context = context;
    }

    public async Task SyncGamesFromApiAsync()
    {
        var gamesFromApi = await _gameClient.GetGames();
        if (!gamesFromApi.Any() || gamesFromApi == null)
            return;
        var apiIds = gamesFromApi.Select(g => g.GameId).Distinct().ToList();
        if (!apiIds.Any())
            return;
        var existingIds = new HashSet<Guid>();
        const int batchSize = 1000;
        for (var i = 0; i < apiIds.Count; i += batchSize)
        {
            var batch = apiIds.Skip(i).Take(batchSize).ToList();
            var found = await _context.Games
                .Where(db => batch.Contains(db.GameId))
                .Select(db => db.GameId)
                .ToListAsync();

            foreach (var id in found)
                existingIds.Add(id);
        }
        var newGames = gamesFromApi
            .Where(g => !existingIds.Contains(g.GameId))
            .ToList();
        if (!newGames.Any())
            return;
        await _context.Games.AddRangeAsync(newGames);
        await _context.SaveChangesAsync();
    }
    
    public async Task SyncGenresFromApiAsync()
    {
        var genresFromApi = await _gameClient.GetGenres();
        if (!genresFromApi.Any() || genresFromApi == null)
            return;
        var genreIds = genresFromApi
            .Select(g => g.Id)
            .Distinct()
            .ToList();
        if (!genreIds.Any())
            return;
        var existingGenreIds = new HashSet<int>();
        const int batchSize = 1000;
        for (var i = 0; i < genreIds.Count; i += batchSize)
        {
            var batch = genreIds.Skip(i).Take(batchSize).ToList();
            var found = await _context.Genres
                .Where(db => batch.Contains(db.Id))
                .Select(db => db.Id)
                .ToListAsync();

            foreach (var name in found)
                existingGenreIds.Add(name);
        }
        var newGenres = genresFromApi
            .Where(g => !existingGenreIds.Contains(g.Id))
            .ToList();
        if (!newGenres.Any())
            return;
        await _context.Genres.AddRangeAsync(newGenres);
        await _context.SaveChangesAsync();
    }
}