using System.Text;
using System.Text.Unicode;
using Currencey.Api;
using Currencey.Api.Database;
using Currencey.Api.Endpoints;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using SystemClock = Microsoft.Extensions.Internal.SystemClock;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddDataBase(config.GetConnectionString("NpgsDb")!);
builder.Services.AddRepositories();

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddCookie(options =>
{
    options.Cookie.HttpOnly = true;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.Name = "X-Auth-Token";
    options.ExpireTimeSpan = TimeSpan.FromDays(7);
    options.SlidingExpiration = true;
}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new ()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = config["Jwt:Issuer"],
        ValidAudience = config["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
        
        
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddOutputCache(o=>
{
    o.AddBasePolicy(c=>c.Cache());
    o.AddPolicy("currencyCache", c =>
    {
        c.Cache();
        c.Expire(TimeSpan.FromHours(1));
        c.Tag("currency");
    });
});

builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var dbInitializer = app.Services.GetRequiredService<DBInitializer>();
await dbInitializer.InitializeAsync();

app.UseHttpsRedirection();

app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

app.MapCurrency();
app.MapAuth();

app.Run();
