namespace GamerLog.Settings;

public class TwitchApiSettings
{
    public string ClientId { get; set; }
    public string ClientSecret { get; set; }
    public string BaseUrl { get; set; }

    public TwitchApiSettings(string clientId, string clientSecret, string baseUrl)
    {
        ClientId = clientId;
        ClientSecret = clientSecret;
        BaseUrl = baseUrl;
    }
    
    public TwitchApiSettings(){}
}