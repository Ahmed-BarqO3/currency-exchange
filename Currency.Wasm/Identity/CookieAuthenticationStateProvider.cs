using System.Net.Http.Json;
using System.Security.Claims;
using System.Text.Json;
using Currencey.Contact;
using Currencey.Contact.Requset;
using Currency.Wasm.Models;
using Microsoft.AspNetCore.Components.Authorization;
namespace Currency.Blazor.Identity;


public class CookieAuthenticationStateProvider(IHttpClientFactory httpClientFactory) : AuthenticationStateProvider, IAccountManagment
{
    bool _authenticated;
    readonly ClaimsPrincipal _unAuthenticated = new ClaimsPrincipal(new ClaimsIdentity());

    readonly JsonSerializerOptions _jsonSerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    readonly HttpClient _httpClient = httpClientFactory.CreateClient("Auth");

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        _authenticated = false;

        var user = _unAuthenticated;

        try
        {
            var userResponse = await _httpClient.GetAsync(ApiRoute.currentUser);
            userResponse.EnsureSuccessStatusCode();

            var userJson = await userResponse.Content.ReadAsStringAsync();
            var userInfo = JsonSerializer.Deserialize<UserInfo>(userJson, _jsonSerializerOptions);

            if (userInfo is not null)
            {
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.Email, userInfo.username),
                };

                var identity = new ClaimsIdentity(claims, nameof(CookieAuthenticationStateProvider));
                user = new ClaimsPrincipal(identity);
                _authenticated = true;
            }
        }
        catch
        {
            //logging
        }

        return new AuthenticationState(user);
    }

    public async Task<AuthResult> LoingAsync(LoginRequset loginModel)
    {
        try
        {
            var Result = await _httpClient.PostAsJsonAsync(ApiRoute.login, new
            {
                loginModel.username,
                loginModel.password,
            });

            if (Result.IsSuccessStatusCode)
            {
                NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
                return new AuthResult { Succeede = true };
            }

        }
        catch
        {
            //logging
        }

        return new AuthResult
        {
            Succeede = false,
            ErrorList = ["Invalid username or password"]
        };
    }

    public async Task LogoutAsync()
    {
        await _httpClient.PostAsync(ApiRoute.logout, content: null);
        NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
    }

}
