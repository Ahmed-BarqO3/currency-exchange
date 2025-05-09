namespace Currencey.Api.Entity;

public class RefreshToken
{
    public string token { get; set; }
    public string jwtid { get; set; }
    public DateTime creation { get; set; }
    public DateTime expiration { get; set; }
    public bool used { get; set; }
    public bool invalidate { get; set; }
    public Guid userid { get; set; }
}
