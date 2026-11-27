using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SaudiPlus.Application.Interfaces;
namespace SaudiPlus.Infrastructure.Identity;
public sealed class JwtOptions
{
    public string Issuer { get; set; } = "SaudiPlus";
    public string Audience { get; set; } = "SaudiPlus.Api";
    public string SigningKey { get; set; } = "";
    public int LifetimeMinutes { get; set; } = 15;
    public TokenValidationParameters ValidationParameters() => new()
    {
        ValidateIssuerSigningKey = true, IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(SigningKey)),
        ValidateIssuer = true, ValidIssuer = Issuer, ValidateAudience = true, ValidAudience = Audience,
        ValidateLifetime = true, RequireExpirationTime = true, RequireSignedTokens = true,
        ValidAlgorithms = [SecurityAlgorithms.HmacSha256], ClockSkew = TimeSpan.Zero,
        NameClaimType = "sub", RoleClaimType = "role"
    };
}
public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public AccessToken Create(Guid id, string email, IEnumerable<string> roles)
    {
        var settings = options.Value;
        var now = DateTime.UtcNow;
        var expires = now.AddMinutes(settings.LifetimeMinutes);
        var claims = new List<Claim> { new("sub", id.ToString()), new("email", email), new("jti", Guid.NewGuid().ToString()) };
        claims.AddRange(roles.Select(role => new Claim("role", role)));
        var token = new JwtSecurityToken(settings.Issuer, settings.Audience, claims, now, expires,
            new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(settings.SigningKey)), SecurityAlgorithms.HmacSha256));
        return new AccessToken(new JwtSecurityTokenHandler().WriteToken(token), expires);
    }
}
