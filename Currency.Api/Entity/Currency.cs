namespace Currencey.Api.Entity;

public class Currency
{
    public string id { get; set; }
    public string name { get; set; }
    public string symbol { get; set; } 
    public decimal amount { get; set; }
    public string password { get; set; }
    public DateTime update_at { get; set; }
}
