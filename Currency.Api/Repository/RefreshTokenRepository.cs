using Currencey.Api.Database;
using Currencey.Api.Entity;
using Dapper;

namespace Currencey.Api.Repository;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly IDbConnectionFactory _db;

    public RefreshTokenRepository(IDbConnectionFactory db)
    {
        _db = db;
    }

    public async Task<bool> AddAsync(RefreshToken refresh_tokens, CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        var sql = $"""
                   INSERT INTO refresh_tokens (token, jwtid, creation, expiration, used, invalidate, userid)
                   VALUES (@token, @jwtid, @creation, @expiration, @used, @invalidate, @userid);
                   """;
        return await connection.ExecuteAsync(
            new CommandDefinition(
                sql,
                new
                {
                    token = refresh_tokens.token,
                    jwtid = refresh_tokens.jwtid,
                    creation = refresh_tokens.creation,
                    expiration = refresh_tokens.expiration,
                    used = refresh_tokens.used,
                    invalidate = refresh_tokens.invalidate,
                    userid = refresh_tokens.userid
                },
                cancellationToken: cancellationToken
            )
        ) > 0;
    }

    public async Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        var sql = $"""
                   Select true From refresh_tokens
                   WHERE token = @token and expiration > now();
                   """;
        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(sql, new { token }, cancellationToken: cancellationToken));
    }

    public async Task<bool> DeleteRefreshTokenForUserAsync(Guid userid, CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        var sql = $"""
                   DELETE FROM refresh_tokens WHERE userid = @userid;
                   """;
        return await connection.ExecuteAsync(new CommandDefinition(sql, new { userid }, cancellationToken: cancellationToken)) > 0;
    }

    public async Task<string?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default)
    {
        using var connection = await _db.CreateConnectionAsync(cancellationToken);
        var sql = $"""
                   SELECT token FROM refresh_tokens WHERE token = @token;
                   """;
        return await connection.QueryFirstOrDefaultAsync<string>(new CommandDefinition(sql, new { token }, cancellationToken: cancellationToken));
    }
}
