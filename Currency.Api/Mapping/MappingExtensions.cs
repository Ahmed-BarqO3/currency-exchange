using Currencey.Contact.Requset;
using Currencey.Contact.Response;
using Currency.Contact.Requset;

namespace Currencey.Api.Mapping;

public static class MappingExtensions
{
    public static CurrencyResponse ToCurrencyResponse(this Entity.Currency currency)
    {
        return new CurrencyResponse(currency.id, currency.name, currency.symbol, currency.amount, currency.update_at);
    }
    public static IEnumerable<CurrencyResponse> ToCurrencyResponse(this IEnumerable<Entity.Currency> currencies)
    {
        return currencies.Select(c => c.ToCurrencyResponse());
    }

    public static Entity.Currency ToCurrency(this ChangePasswordRequset requset)
    {
        return new Entity.Currency
        {
            password = requset.newPassword
        };
    }

    public static IEnumerable<Entity.Currency>  ToCurrencies(this UpdateCurrenciesRequset requset)
    {
        return requset.items.Select(x => x.ToCurrency());
    }


    static Entity.Currency ToCurrency(this UpdateCurrencyRequset requset)
    {
        return new Entity.Currency
        {
            id = requset.id,
            amount = requset.amount,
            update_at = DateTime.UtcNow.AddHours(2)
        };
     }
}




