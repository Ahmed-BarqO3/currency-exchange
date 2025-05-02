using System.Security.Claims;
using System.Text;
using System.Text.Json;
using Currencey.Contact;
using Currencey.Contact.Requset;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;

namespace Currencey.Api.Endpoints;

public static class AuthEndpoint
{
    public static void MapAuth(this IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoute.login, CreateToken);
    }
    
    static async Task<IResult> CreateToken(IConfiguration config,LoginRequset request,bool useCookie = default,CancellationToken cancellationToken = default)
    {
        var token = new JsonWebTokenHandler();
        var key = config["Jwt:Key"];

        var claims = new List<Claim>
        {
            new (JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new (JwtRegisteredClaimNames.PreferredUsername, request.username),
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
        
        if (useCookie)
        {
            var httpContext = new DefaultHttpContext();
            httpContext.Response.Cookies.Append("X-Auth-Token", jwt,new CookieOptions()
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = DateTimeOffset.UtcNow.AddHours(1)
            });
            
            return Results.Ok();
        }
        return Results.Ok(jwt);
    }
    
}
