using System.Text;
using Currencey.Api;
using Currencey.Api.Database;
using Currencey.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddDataBase(config.GetConnectionString("NpgsDb")!);
builder.Services.AddRepositories();

builder.Services.AddAuthentication(i =>
{
    i.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    i.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    i.DefaultSignInScheme = CookieAuthenticationDefaults.AuthenticationScheme;

}).AddJwtBearer(x =>
{
    x.RequireHttpsMetadata = false;
    x.SaveToken = true;
    x.TokenValidationParameters = new()
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

    x.Events = new();
    x.Events.OnMessageReceived = context =>
    {

        if (context.Request.Cookies.TryGetValue("X-Access-Token", out var token))
        {
            context.Token = token;
        }
        return Task.CompletedTask;
    };
}).AddCookie(options =>
{
    options.Cookie.Name = "X-Access-Token";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.None;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;

    options.Events.OnSigningOut = context =>
    {
        context.Response.Cookies.Delete("X-Refresh-Token");
        return Task.CompletedTask;
    };

});
builder.Services.AddCors(builder =>
{
    builder.AddDefaultPolicy(options =>
    {
        options.WithOrigins(config.GetValue<string>("client")!)
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOutputCache(o =>
{
    o.AddPolicy("currencyCache", c =>
    {
        c.Cache();
        c.Expire(TimeSpan.FromHours(1));
        c.Tag("currency");
    });
});
builder.Services.AddHealthChecks();
builder.Services.AddAuthorization();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
}

var dbInitializer = app.Services.GetRequiredService<DBInitializer>();
await dbInitializer.InitializeAsync();

app.MapHealthChecks("_health");
app.UseHttpsRedirection();

app.UseExceptionHandler(app =>
{
    app.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        var contextFeature = context.Features.Get<IExceptionHandlerFeature>();
        if (contextFeature is not null)
        {
            await context.Response.WriteAsJsonAsync(new
            {
                StatusCode = context.Response.StatusCode,
                Message = "Internal Server Error"
            });
        }
    });
});

app.UseCors();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

app.MapCurrency();
app.MapAuth();

app.Run();
