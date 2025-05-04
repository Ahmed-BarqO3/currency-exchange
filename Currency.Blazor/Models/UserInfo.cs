namespace Currency.Blazor.Models;

public class UserInfo
{
    public string username { get; set; } = string.Empty;
    public Dictionary<string, string> CustomClaims { get; set; } = [];
}
