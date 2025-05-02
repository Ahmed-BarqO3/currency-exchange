namespace Currencey.Contact.Response;

public record CurrencyResponse(string id, string name, string symbol, decimal Rate, DateTime update_at);
