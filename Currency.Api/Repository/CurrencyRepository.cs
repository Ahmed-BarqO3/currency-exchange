using Currencey.Api.Database;
using Dapper;

namespace Currencey.Api.Repository;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IDbConnectionFactory _db;

    public CurrencyRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<bool> UpdateCurrencyAsync(List<Entity.Currency> currency, CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition("""
                                                                   UPDATE currency Set amount = @amount,
                                                                                       update_at = now() + interval '2 hour'
                                                                   WHERE id = @id
                                                                   """, parameters: currency,cancellationToken: cancellationToken)) > 0;

    }

    public async Task<IEnumerable<Entity.Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<Entity.Currency>(new CommandDefinition("""
                                                                           SELECT * FROM currency
                                                                           """,cancellationToken: cancellationToken));
    }
}
