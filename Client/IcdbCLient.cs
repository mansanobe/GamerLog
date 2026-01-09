using System.Collections.Concurrent;
using System.Diagnostics;
using System.Text;
using System.Text.Json.Serialization;
using GamerLog.Configuration;
using GamerLog.Data.DTO;
using GamerLog.Entities;
using GamerLog.Settings;
using Microsoft.Extensions.Options;

namespace GamerLog.Client;

public class IcdbClient : IGameClient
{
    private readonly HttpClient _client;
    private readonly IcbdTokenConfig _tokenConfig;
    private readonly string _clientId;

    private string? _accessToken;
    private long _tokenExpiryUnix;

    public IcdbClient(HttpClient client, IcbdTokenConfig tokenConfig, IOptions<TwitchApiSettings> settings)
    {
        _client = client;
        _tokenConfig = tokenConfig;
        _clientId = settings.Value.ClientId; 
    }
    
    public void SetToken(string newToken, long expiresInSeconds)
    {
        _accessToken = newToken;
        _tokenExpiryUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresInSeconds;
    }

    public async Task<List<DigitalGame>> GetGames()
    {
        await EnsureValidTokenAsync();
        
        int totalGames = await GetTotalGameCountAsync();
        
        if (totalGames == 0) return new List<DigitalGame>();

        var allGames = new ConcurrentBag<DigitalGame>();
        
        using var semaphore = new SemaphoreSlim(3);
        
        var tasks = new List<Task>();
        
        const int limit = 500;
        
        for (int offset = 0; offset < totalGames; offset += limit)
        {
            int currentOffset = offset;

            tasks.Add(Task.Run(async () => 
            {
                await semaphore.WaitAsync(); 
                try
                {
                    var query = $"fields name, genres.name, first_release_date, cover.url; limit {limit}; offset {currentOffset};";
                    var request = CreateRequest("games", query);
                
                    var response = await _client.SendAsync(request);
                
                    // Se der erro 429 (Too Many Requests), a gente deveria tratar com retry,
                    // mas aqui vamos apenas garantir sucesso ou falhar.
                    response.EnsureSuccessStatusCode();

                    var games = await response.Content.ReadFromJsonAsync<List<IgdbGameDto>>();
                    var digitalGames = games?.ConvertAll(g =>
                    {
                        return g.ToEntity();
                    });

                    if (digitalGames == null) return;
                    foreach (var game in digitalGames)
                    {
                        allGames.Add(game);
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        await Task.WhenAll(tasks);

        return allGames.ToList();
    }

    public async Task<List<Genre>> GetGenres()
    {
        await EnsureValidTokenAsync();
        int totalGames = await GetTotalGenreCountAsync();
        
        if (totalGames == 0) return new List<Genre>();
        var tasks = new List<Task>();
        const int limit = 500;
        var query = $"fields name; limit {limit};";
        
        var request = CreateRequest("genres", query);
        var response = await _client.SendAsync(request);
        response.EnsureSuccessStatusCode();
        
        var genres = await response.Content.ReadFromJsonAsync<List<IgdbGenreDto>>();
        var digitalGenres = genres?.ConvertAll(g =>
        {
            return g.ToEntity();
        });
        if (digitalGenres == null) return new List<Genre>();
        return digitalGenres;
    }
    
    private async Task<int> GetTotalGameCountAsync()
    {
        var request = CreateRequest("games/count", "");
        var response = await _client.SendAsync(request);
    
        if (!response.IsSuccessStatusCode) return 0;

        var result = await response.Content.ReadFromJsonAsync<CountResponse>();
        return result?.Count ?? 0;
    }
    
    private async Task<int> GetTotalGenreCountAsync()
    {
        var request = CreateRequest("genres/count", "");
        var response = await _client.SendAsync(request);
    
        if (!response.IsSuccessStatusCode) return 0;

        var result = await response.Content.ReadFromJsonAsync<CountResponse>();
        return result?.Count ?? 0;
    }

    private Task EnsureValidTokenAsync()
    {
        TwitchAuthResponse response;
        var currentTime = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        if (!string.IsNullOrEmpty(_accessToken) || currentTime <= (_tokenExpiryUnix - 60))
        {
            return Task.CompletedTask;
        }
        response = _tokenConfig.GetIcdbTokenAsync().Result;
        this._accessToken = response.Token;
        this._tokenExpiryUnix = currentTime + response.TokenExpiryUnix;
        return Task.CompletedTask;
    }

    private HttpRequestMessage CreateRequest(string endpoint, string queryBody)
    {
        var request = new HttpRequestMessage(HttpMethod.Post, endpoint);
        
        request.Headers.Add("Client-ID", _clientId);
        request.Headers.Add("Authorization", $"Bearer {_accessToken}");
        
        request.Content = new StringContent(queryBody, Encoding.UTF8, "text/plain");

        return request;
    }
    public record TwitchAuthResponse(string Token,long TokenExpiryUnix);

    private record CountResponse([property: JsonPropertyName("count")] int Count);
}