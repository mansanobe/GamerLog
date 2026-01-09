namespace GamerLog.Settings;

public class IcdbApiSettings
{
    public string BaseUrl { get; set; }
    public string Token { get; set; }
    public long TokenExpiryTime { get; set; }
    
    public IcdbApiSettings(string baseUrl, string token)
    {
        this.BaseUrl = baseUrl;
        this.Token = token;
        this.TokenExpiryTime = 0;
    }
    
    public IcdbApiSettings(){}
}