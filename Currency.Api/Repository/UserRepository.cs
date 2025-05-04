using Currencey.Api.Database;
using Currencey.Api.Entity;
using Dapper;

namespace Currencey.Api.Repository;

public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _db;

    public UserRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<User?> GetByUsernameAsync(string username,CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        return await connection.QueryFirstAsync<User?>(new CommandDefinition($"""
                                                                        SELECT * FROM users
                                                                        WHERE username = @username
                                                                        """, new {username},cancellationToken: cancellationToken));
    }
}
