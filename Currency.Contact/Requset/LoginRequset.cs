namespace Currencey.Contact.Requset;

public record LoginRequset(string username, string password, Dictionary<string,object> CustomClaims);
