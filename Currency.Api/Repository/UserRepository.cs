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

    public async Task<bool> ChangePasswordAsync(string username, string password, CancellationToken token = default)
    {
        using var connection = await _db.CreateConnectionAsync(token);
        return await connection.ExecuteAsync(new CommandDefinition($"""
                                UPDATE users Set password = @password
                                WHERE username = @username
                                """, new { username, password  }, cancellationToken: token)) > 0;
    }   

    public async Task<User?> GetByUsernameAsync(string username,CancellationToken token = default)
    {
        using var connection = await _db.CreateConnectionAsync(token);
        return await connection.QueryFirstOrDefaultAsync<User?>(new CommandDefinition($"""
                                SELECT * FROM users
                                WHERE username = @username
                                """, new { username }, cancellationToken: token));
    }
}
