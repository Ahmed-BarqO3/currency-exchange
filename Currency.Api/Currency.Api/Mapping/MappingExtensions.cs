using Currencey.Api.Entity;
using Currencey.Contact.Response;

namespace Currencey.Api.Mapping;

public static class MappingExtensions
{
    public static CurrencyResponse ToCurrencyResponse(this Currency currency)
    {
        return new CurrencyResponse(currency.id, currency.name, currency.symbol, currency.amount, currency.update_at);
    }
    public static IEnumerable<CurrencyResponse> ToCurrencyResponse(this IEnumerable<Currency> currencies)
    {
        return currencies.Select(c => c.ToCurrencyResponse());
    }
}




