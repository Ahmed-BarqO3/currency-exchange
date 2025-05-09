namespace Currencey.Contact.Requset;

public record RefreshTokenRequset
{
    public string? RefreshToken { get; set; }
    public string? AccessToken { get; set; }
}
