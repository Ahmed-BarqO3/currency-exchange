using Currencey.Api.Entity;

namespace Currencey.Api.Repository;

public interface IUserRepository
{
    Task<bool> ChangePasswordAsync(string username, string newPassword, CancellationToken token = default);
    Task<User?> GetByUsernameAsync(string username,CancellationToken token = default);
}
