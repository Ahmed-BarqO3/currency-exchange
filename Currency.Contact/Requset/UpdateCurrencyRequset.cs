namespace Currency.Contact.Requset;
public record UpdateCurrencyRequset
{
    public string id { get; set; }
    public decimal amount { get; set; }

    public UpdateCurrencyRequset(string id, decimal amount)
    {
        this.id = id;
        this.amount = amount;
    }
}


public class UpdateCurrenciesRequset
{
    public IEnumerable<UpdateCurrencyRequset> items { get; set; } = [];
}
