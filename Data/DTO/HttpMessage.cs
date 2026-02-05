namespace GamerLog.Data.DTO;

public class HttpMessage
{
    public string Message { get; set; }
    public string Code { get; set; }

    public override string ToString()
    {
        return $"Code: {Code}\nMessage: {Message}";
    }
}