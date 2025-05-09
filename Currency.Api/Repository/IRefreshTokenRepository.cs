using Currencey.Api.Entity;

namespace Currencey.Api.Repository;

public interface IRefreshTokenRepository
{
    Task<bool> AddAsync(RefreshToken refresh_tokens, CancellationToken cancellationToken = default);
    Task<bool> ValidateTokenAsync(string token, CancellationToken cancellationToken = default);
    Task<bool> DeleteRefreshTokenForUserAsync(Guid userid, CancellationToken cancellationToken = default);
    Task<string?> GetRefreshTokenAsync(string token, CancellationToken cancellationToken = default);
}
