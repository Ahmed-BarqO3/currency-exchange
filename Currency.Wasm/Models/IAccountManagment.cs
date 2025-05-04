using Currencey.Contact.Requset;

namespace Currency.Wasm.Models;

public interface IAccountManagment
{
    Task<AuthResult> LoingAsync(LoginRequset loginModel);
    Task LogoutAsync();
}
