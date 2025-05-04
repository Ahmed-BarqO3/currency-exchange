namespace Currencey.Contact.Requset;

public record LoginRequset
{
    public  string username { get; set; } 
    public string password { get; set; }
    public Dictionary<string, object> CustomClaims { get; set; } = [];

}


