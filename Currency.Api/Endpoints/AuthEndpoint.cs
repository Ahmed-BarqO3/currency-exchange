using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Currencey.Api.Entity;
using Currencey.Api.Repository;
using Currencey.Contact;
using Currencey.Contact.Requset;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace Currencey.Api.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoute.login, Login);

        app.MapPost(ApiRoute.logout, Logout)
            .RequireAuthorization();
        app.MapGet(ApiRoute.currentUser, GetUser)
            .RequireAuthorization();

        app.MapPost(ApiRoute.refreshToken, RefreshToken);

        app.MapPut(ApiRoute.changePassword, ChangePassword)
            .RequireAuthorization();
    }


    static async Task<Results<Ok, BadRequest>> ChangePassword(HttpContext http, [FromBody] ChangePasswordRequset requset,
        [FromServices] IUserRepository userRepository, CancellationToken token = default)
    {
        var username = http.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;

        var user = await userRepository.GetByUsernameAsync(username!, token);


        var res = new PasswordHasher<User>().VerifyHashedPassword(user!, user!.password, requset.oldPassword);
        if (res == PasswordVerificationResult.Failed) return TypedResults.BadRequest();

        user!.password = new PasswordHasher<User>().HashPassword(user, requset.newPassword);
        var result = await userRepository.ChangePasswordAsync(user.username, user.password, token);
        return result ? TypedResults.Ok() : TypedResults.BadRequest();
    }


    static async Task<Results<Ok<User>, NotFound>> GetUser(HttpContext http, [FromServices] IUserRepository userRepository, CancellationToken token = default)
    {
        var username = http.User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value;
        if (string.IsNullOrEmpty(username))
            return TypedResults.NotFound();

        var user = await userRepository.GetByUsernameAsync(username, token);
        return TypedResults.Ok(user);
    }

    static async Task<IResult> Logout(HttpContext context, CancellationToken cancellationToken = default)
    {
        await context.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return Results.NoContent();
    }

    static async Task<Results<Ok, BadRequest>> Login(IConfiguration config, [FromBody] LoginRequset request, [FromServices] IUserRepository userRepository, [FromServices] IRefreshTokenRepository tokenRepository, HttpContext context, CancellationToken cancellationToken = default)
    {
        var user = await userRepository.GetByUsernameAsync(request.username, cancellationToken);

        if (user is null)
            return TypedResults.BadRequest();

        var hash = new PasswordHasher<User>().VerifyHashedPassword(user, user.password, request.password);
        if (hash == PasswordVerificationResult.Failed)
            return TypedResults.BadRequest();

        var token = new JsonWebTokenHandler();
        var key = config["Jwt:Key"];

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Name, request.username),
            new (JwtRegisteredClaimNames.NameId, user.id.ToString())

        };


        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var principal = new ClaimsPrincipal(identity);

        var jwt = token.CreateToken(tokenDescriptor);

        await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                                    new AuthenticationProperties { IsPersistent = false });

        var refreshToken = new RefreshToken
        {
            token = Guid.NewGuid().ToString(),
            jwtid = Guid.NewGuid().ToString(),
            creation = DateTime.UtcNow,
            expiration = DateTime.UtcNow.AddDays(7),
            used = false,
            invalidate = false,
            userid = user.id
        };

        await tokenRepository.AddAsync(refreshToken, cancellationToken);

        context.Response.Cookies.Append("X-Refresh-Token", refreshToken.token,
            new CookieOptions() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, });

        return TypedResults.Ok();
    }

    static async Task<Results<Ok, BadRequest>> RefreshToken(HttpContext context, [FromServices] IRefreshTokenRepository tokenRepository, [FromServices] IConfiguration config, CancellationToken token = default)
    {

        RefreshTokenRequset request = new()
        {
            AccessToken = context.Request.Cookies.TryGetValue("X-Access-Token", out var accessToken) ? accessToken : string.Empty,
            RefreshToken = context.Request.Cookies.TryGetValue("X-Refresh-Token", out var refreshToken) ? refreshToken : string.Empty
        };



        var principal = await GetPrincipalFromExpiredToken(request.AccessToken, config);
        if (principal is not null)
        {

            if (await tokenRepository.ValidateTokenAsync(request.RefreshToken))
            {
                var TokenHandler = new JsonWebTokenHandler();
                var key = config["Jwt:Key"];
                var claims = new List<Claim>
                {
                    new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new (JwtRegisteredClaimNames.Name, principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Name)?.Value),
                    new (JwtRegisteredClaimNames.NameId, principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value)
                };

                var tokenDescriptor = new SecurityTokenDescriptor
                {
                    Subject = new ClaimsIdentity(principal.Claims),
                    Issuer = config["Jwt:Issuer"],
                    Audience = config["Jwt:Audience"],
                    Expires = DateTime.Now.AddHours(1),
                    SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                        SecurityAlgorithms.HmacSha256Signature)
                };

                var jwt = TokenHandler.CreateToken(tokenDescriptor);
                RefreshToken newRefreshToken = new RefreshToken
                {
                    token = Guid.NewGuid().ToString(),
                    jwtid = Guid.NewGuid().ToString(),
                    creation = DateTime.UtcNow,
                    expiration = DateTime.UtcNow.AddDays(7),
                    used = false,
                    invalidate = false,
                    userid = Guid.TryParse(
                        principal.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.NameId)?.Value,
                        out var id)
                        ? id
                        : Guid.Empty
                };

                if (await tokenRepository.AddAsync(newRefreshToken, token))

                    await context.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal,
                       new AuthenticationProperties { IsPersistent = false });

                context.Response.Cookies.Append("X-Refresh-Token", newRefreshToken.token,
                    new CookieOptions() { HttpOnly = true, Secure = true, SameSite = SameSiteMode.None, });


                await tokenRepository.DeleteRefreshTokenAsync(request.RefreshToken, token);

                return TypedResults.Ok();
            }
        }
        return TypedResults.BadRequest();
    }

    async static Task<ClaimsPrincipal?> GetPrincipalFromExpiredToken(string token, IConfiguration config)
    {
        try
        {
            var tokenHandler = new JsonWebTokenHandler();
            var validationResult = await tokenHandler.ValidateTokenAsync(token, new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["Jwt:Issuer"],
                ValidAudience = config["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(config["Jwt:Key"]!)),
            });

            if (validationResult.IsValid && validationResult.ClaimsIdentity is not null)
            {
                return new ClaimsPrincipal(validationResult.ClaimsIdentity);
            }
        }
        catch
        {
            return null;
        }

        return null;
    }
}
