using Currencey.Api.Entity;

namespace Currencey.Api.Repository;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username,CancellationToken cancellationToken = default);
}
