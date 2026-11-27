using System.Security.Claims;
using SaudiPlus.Application.Interfaces;
namespace SaudiPlus.Api;
public sealed class HttpCurrentUser(IHttpContextAccessor accessor) : ICurrentUser
{
    private ClaimsPrincipal? Principal => accessor.HttpContext?.User;
    public Guid? UserId => Principal?.Identity?.IsAuthenticated == true &&
        Guid.TryParse(Principal.FindFirstValue("sub"), out var id) ? id : null;
    public bool IsInRole(string role) => Principal?.Identity?.IsAuthenticated == true &&
        Principal.HasClaim("role", role);
}
