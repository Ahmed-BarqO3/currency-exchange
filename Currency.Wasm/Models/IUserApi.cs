using Currencey.Contact;
using Currencey.Contact.Requset;
using Refit;

namespace Currency.Wasm.Models;

public interface IUserApi
{
    [Put("/"+ApiRoute.changepassword)]
    Task<HttpResponseMessage> ChangePasswordAsync(ChangePasswordRequset requset, CancellationToken token = default);
}
