namespace Currencey.Api.Repository;

public interface ICurrencyRepository
{
    Task<bool> UpdateCurrencyAsync(List<Entity.Currency> currencies,CancellationToken cancellationToken = default);
    Task<IEnumerable<Entity.Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default);
}


