using System.Collections.Concurrent;
using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using GamerLog.Configuration;
using GamerLog.Data.DTO;
using GamerLog.Entities;
using GamerLog.Settings;
using Microsoft.Extensions.Options;
using Polly;
using Polly.Retry;

namespace GamerLog.Client;

public class IcdbClient : IGameClient
{
    private readonly HttpClient _client;
    private readonly IcbdTokenConfig _tokenConfig;
    private readonly string _clientId;
    private readonly AsyncRetryPolicy<HttpResponseMessage> _retryPolicy;
    private readonly ILogger<IcdbClient> _logger;
    
    private string? _accessToken;
    private long _tokenExpiryUnix;

    public IcdbClient(HttpClient client, IcbdTokenConfig tokenConfig, IOptions<TwitchApiSettings> settings, ILogger<IcdbClient> logger)
    {
        _logger = logger;
        _client = client;
        _tokenConfig = tokenConfig;
        _clientId = settings.Value.ClientId;
        
        _retryPolicy = Policy
            .HandleResult<HttpResponseMessage>(r=> 
                r.StatusCode == HttpStatusCode.TooManyRequests ||
                (int)r.StatusCode > 500)
            .Or<HttpRequestException>()
            .WaitAndRetryAsync(3, retryAttempt =>
                TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (_, timespan, retryCount, _) =>
                {
                    logger.LogInformation("Retrying ICDB request. Retry attempt {RetryCount}, waiting {Delay} before next retry. ", retryCount, timespan);
                });
    }
    
    public void SetToken(string newToken, long expiresInSeconds)
    {
        _accessToken = newToken;
        _tokenExpiryUnix = DateTimeOffset.UtcNow.ToUnixTimeSeconds() + expiresInSeconds;
    }

    public async Task<List<Game>> GetGames(int dbGameCount, int lastKnownIgdbId)
    {
        await EnsureValidTokenAsync();
        
        _logger.LogInformation("{Date}:Getting last game id in DB.", DateTime.Now);
        int lastGamesId = await GetLastIdAsync();
        _logger.LogInformation("{Date}:Last game id in ICDB - {LastGameId}.", DateTime.Now, lastGamesId);

        int rangeSize = 50000;
        
        if (await GetTotalGameCountAsync() <= dbGameCount) return new List<Game>();

        var allGames = new ConcurrentBag<Game>();
        
        using var semaphore = new SemaphoreSlim(3);
        
        var tasks = new List<Task>();
        
        const int limit = 500;
        
        for (int startId = lastKnownIgdbId; startId < lastGamesId; startId += rangeSize)
        {
            int rangeStart = startId;
            int rangeEnd = startId + rangeSize;

            tasks.Add(Task.Run(async () => 
            {
                await semaphore.WaitAsync(); 
                try
                {
                    int currentCursor = rangeStart;
                    bool hasMoreInRange = true;

                    while (hasMoreInRange)
                    {
                        var response = await _retryPolicy.ExecuteAsync(async () =>
                        {
                            var query = $"fields name, genres.name, first_release_date, cover.url; " +
                                        $"where id > {currentCursor} & id <= {rangeEnd}; " +
                                        $"sort id asc;limit {limit};";
                            var request = CreateRequest("games", query);
                            return await _client.SendAsync(request);
                        });


                        response.EnsureSuccessStatusCode();
                        
                        var gamesDto = await response.Content.ReadFromJsonAsync<List<IgdbGameDto>>();

                        if (!gamesDto!.Any() || gamesDto == null)
                        {
                            hasMoreInRange = false;
                            break;
                        }
                        
                        foreach (var gameDto in gamesDto) allGames.Add(gameDto.ToEntity());
                
                        currentCursor = gamesDto.Max(x => x.IgdbId);
                        if (gamesDto.Count < limit) hasMoreInRange = false;
                    }
                }
                finally
                {
                    semaphore.Release();
                }
            }));
        }
        _logger.LogInformation("{Date}:Fetching games from ICDB in {TaskCount} tasks.", DateTime.Now, tasks.Count);
        await Task.WhenAll(tasks);
        _logger.LogInformation("{Date}:Completed fetching games from ICDB. Total games fetched: {GameCount}.", DateTime.Now, allGames.Count);

        return allGames.ToList();
    }

    public async Task<List<Genre>> GetGenres(int dbGenresCount)
    {
        await EnsureValidTokenAsync();
        int totalGames = await GetTotalGenreCountAsync();
        
        if (totalGames == 0 || totalGames <= dbGenresCount) return new List<Genre>();
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
    
    private async Task<int> GetLastIdAsync()
    {
        var query = $"fields id; sort id desc; limit 1;";
        var request = CreateRequest("games", query);
        var response = await _client.SendAsync(request);
    
        if (!response.IsSuccessStatusCode) return 0;

        var result = await response.Content.ReadFromJsonAsync<List<LastIdResponse>>();
        return result?.FirstOrDefault()?.Id ?? 0;
    }
    
    private async Task<int> GetTotalGameCountAsync()
    {
        var query = "";
        var request = CreateRequest("games/count", query);
        var response = await _client.SendAsync(request);
    
        if (!response.IsSuccessStatusCode) return 0;

        var result = await response.Content.ReadFromJsonAsync<CountResponse>();
        return result?.Count ?? 0;
    }
    
    private async Task<int> GetTotalGenreCountAsync()
    {
        var query = "";
        var request = CreateRequest("genres/count", query);
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
    
    private record LastIdResponse([property: JsonPropertyName("id")] int Id);
}