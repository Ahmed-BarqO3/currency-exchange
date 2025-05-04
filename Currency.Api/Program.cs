using System.Text;
using Currencey.Api;
using Currencey.Api.Database;
using Currencey.Api.Endpoints;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

var config = builder.Configuration;

builder.Services.AddOpenApi();
builder.Services.AddDataBase(config.GetConnectionString("NpgsDb")!);
builder.Services.AddRepositories();

builder.Services.AddAuthentication(i =>
{
    i.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    i.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    i.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
    i.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;

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
    x.Events.OnMessageReceived = context => {
       
            if (string.IsNullOrEmpty(context.Token))
            {
                context.Token = context.Request.Cookies["X-Access-Token"];
            }
            return Task.CompletedTask;
    };
}).AddCookie(options =>
{
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
    options.Cookie.IsEssential = true;

});
builder.Services.AddCors(builder =>
{
    builder.AddDefaultPolicy( options =>
    {
        options.WithOrigins("https://localhost:7126")
            .AllowCredentials()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

builder.Services.AddOutputCache(o =>
{
    o.AddBasePolicy(c => c.Cache());
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

app.UseCors();
app.UseOutputCache();

app.UseAuthentication();
app.UseAuthorization();

app.MapCurrency();
app.MapAuth();

app.Run();
