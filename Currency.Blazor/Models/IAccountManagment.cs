using Currencey.Contact.Requset;
namespace Currency.Blazor.Models;

public interface IAccountManagment
{
    Task<AuthResult> LoingAsync(LoginRequset loginModel);
    Task LogoutAsync();
}
