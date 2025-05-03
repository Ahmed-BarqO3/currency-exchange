using Currencey.Api.Entity;

namespace Currencey.Api.Repository;

public interface ICurrencyRepository
{
    Task<bool> UpdateCurrencyAsync(string id, decimal amount,CancellationToken cancellationToken = default);
    Task<IEnumerable<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
}
