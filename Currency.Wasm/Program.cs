using Blazored.Toast;
using Currency.Blazor.Identity;
using Currency.Wasm;
using Currency.Wasm.Models;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Refit;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");


builder.Services.AddBlazoredToast();
builder.Services.AddTransient<CookieHandler>();
builder.Services.AddScoped<AuthenticationStateProvider, CookieAuthenticationStateProvider>();
builder.Services.AddScoped(sp => (IAccountManagment)sp.GetRequiredService<AuthenticationStateProvider>());

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

builder.Services.AddHttpClient("Auth", client =>
{
    client.BaseAddress = new Uri("https://localhost:7153");
}).AddHttpMessageHandler<CookieHandler>();


builder.Services.AddRefitClient<ICurrencyApi>().ConfigureHttpClient(o =>
{
    o.BaseAddress = new Uri("https://localhost:7153");
}).AddHttpMessageHandler<CookieHandler>();

builder.Services.AddRefitClient<IUserApi>().ConfigureHttpClient(o =>
{
    o.BaseAddress = new Uri("https://localhost:7153");
}).AddHttpMessageHandler<CookieHandler>();


builder.Services.AddAuthorizationCore();

await builder.Build().RunAsync();
