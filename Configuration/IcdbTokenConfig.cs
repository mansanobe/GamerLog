using System.Text.Json.Serialization; 
using GamerLog.Client;
using GamerLog.Settings;
using Microsoft.Extensions.Options;

namespace GamerLog.Configuration;

public class IcbdTokenConfig
{
    private readonly HttpClient _client;
    private readonly TwitchApiSettings _settings;

    public IcbdTokenConfig(HttpClient client, IOptions<TwitchApiSettings> settings)
    {
        _client = client;
        _settings = settings.Value;
    }

    public async Task<IcdbClient.TwitchAuthResponse> GetIcdbTokenAsync()
    { 
        var content = new FormUrlEncodedContent(new[]
        {
            new KeyValuePair<string, string>("client_id", _settings.ClientId),
            new KeyValuePair<string, string>("client_secret", _settings.ClientSecret),
            new KeyValuePair<string, string>("grant_type", "client_credentials")
        });
        var response = await _client.PostAsync("", content);
        
        response.EnsureSuccessStatusCode();
        
        var result = await response.Content.ReadFromJsonAsync<TwitchAuthResponse>();

        if (result == null)
        {
            throw new Exception("Error retrieving ICDB token: Response deserialization failed.");
        }
        return new IcdbClient.TwitchAuthResponse(result.AccessToken, result.ExpiresIn);
    }
    private record TwitchAuthResponse(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] long ExpiresIn,
        [property: JsonPropertyName("token_type")] string TokenType
    );
}