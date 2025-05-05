namespace Currencey.Contact.Response;

public record CurrencyResponse
{
    public string id { get; set; }
    public string name { get; set; }
    public string symbol { get; set; }
    public decimal amount { get; set; }
    public DateTime update_at { get; set; }

    public CurrencyResponse(string id, string name, string symbol, decimal amount, DateTime update_at)
    {
        this.id = id;
        this.name = name;
        this.symbol = symbol;
        this.amount = amount;
        this.update_at = update_at;
    }
}
