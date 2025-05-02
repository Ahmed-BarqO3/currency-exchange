using Currencey.Api.Database;
using Currencey.Api.Entity;
using Dapper;

namespace Currencey.Api.Repository;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IDbConnectionFactory _db;

    public CurrencyRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<bool> UpdateCurrencyAsync(string id, decimal amount,CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        return await connection.ExecuteAsync(new CommandDefinition("""
                                                                   UPDATE currency Set amount = @amount,
                                                                                       update_at = now() + interval ' hour'
                                                                   WHERE id = @id
                                                                   """, new {id, amount},cancellationToken: cancellationToken)) > 0;

    }

    public async Task<IEnumerable<Currency>> GetCurrenciesAsync(CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        return await connection.QueryAsync<Currency>(new CommandDefinition("""
                                                                           SELECT * FROM currency
                                                                           """,cancellationToken: cancellationToken));
    }
}
