using Currencey.Contact;
using Currencey.Contact.Response;
using Currency.Contact.Requset;
using Refit;

namespace Currency.Wasm.Models;


public interface ICurrency
{
    [Put("/"+ApiRoute.Currency.Update)]
    Task Update(UpdateCurrenciesRequset requset,CancellationToken token = default);

    [Get("/"+ApiRoute.Currency.GetAll)]
    Task<List<CurrencyResponse>>GetCurrencies(CancellationToken token = default);
}
