using Currencey.Contact;
using Currencey.Contact.Response;
using Currency.Contact.Requset;
using Refit;

namespace Currency.Wasm.Models;


public interface ICurrencyApi
{
    [Put("/"+ApiRoute.Currency.Update)]
    Task<HttpResponseMessage> Update(UpdateCurrenciesRequset requset,CancellationToken token = default);

    [Get("/"+ApiRoute.Currency.GetAll)]
    Task<List<CurrencyResponse>>GetCurrencies(CancellationToken token = default);
}
