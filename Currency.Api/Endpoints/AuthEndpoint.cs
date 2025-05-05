using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Currencey.Api.Entity;
using Currencey.Api.Repository;
using Currencey.Contact;
using Currencey.Contact.Requset;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Currencey.Api.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoute.login, CreateToken);
        app.MapPost(ApiRoute.logout, Logout);
        app.MapGet(ApiRoute.currentUser, GetUser)
            .RequireAuthorization();
    }



    static async Task<Results<Ok<User>, NotFound>> GetUser(this ClaimsPrincipal claims,HttpContext http,[FromServices]IUserRepository userRepository, CancellationToken token = default)
    {
        var username = http.User.Claims.FirstOrDefault(c=>c.Type == JwtRegisteredClaimNames.Name)?.Value;
        if (string.IsNullOrEmpty(username))
            return TypedResults.NotFound();
        
        var user = await userRepository.GetByUsernameAsync(username, token);
        return TypedResults.Ok(user);
    }
    
    
    static  Task<IResult> Logout(HttpContext context,CancellationToken cancellationToken = default)
    {
        context.Response.Cookies.Delete("X-Access-Token");
        return Task.FromResult(Results.NoContent());
    }

    static async Task<Results<Ok,BadRequest>> CreateToken(IConfiguration config, [FromBody] LoginRequset request,[FromServices] IUserRepository userRepository,HttpContext context,CancellationToken cancellationToken = default)
    {

        var user = await userRepository.GetByUsernameAsync(request.username, cancellationToken);

        if(user is  null || user?.password != request.password)
            return TypedResults.BadRequest();


        var token = new JsonWebTokenHandler();
        var key = config["Jwt:Key"];

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.Name, request.username),
        };

        foreach (var claim in request.CustomClaims)
        {
            var jsonElement = (JsonElement)claim.Value;
            var value = jsonElement.ValueKind switch
            {
                JsonValueKind.Number => ClaimValueTypes.Double,
                JsonValueKind.True => ClaimValueTypes.Boolean,
                JsonValueKind.False => ClaimValueTypes.Boolean,
                _ => ClaimValueTypes.String
            };

            claims.Add(new Claim(claim.Key, claim.Value.ToString()!, value));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Issuer = config["Jwt:Issuer"],
            Audience = config["Jwt:Audience"],
            Expires = DateTime.Now.AddHours(1),
            SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key!)),
                SecurityAlgorithms.HmacSha256Signature)
        };

        var jwt = token.CreateToken(tokenDescriptor);
        
            context.Response.Cookies.Append("X-Access-Token", jwt,new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
            });
            return TypedResults.Ok();
    }
    
}
