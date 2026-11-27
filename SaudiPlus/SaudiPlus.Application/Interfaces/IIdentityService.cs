namespace SaudiPlus.Application.Interfaces;
public sealed record AccessToken(string Token, DateTime ExpiresAtUtc);
public interface IJwtTokenService { AccessToken Create(Guid id, string email, IEnumerable<string> roles); }
public interface IIdentityService
{
    Task<bool> RegisterAsync(string email, string password, CancellationToken cancellationToken = default);
    Task<AccessToken?> LoginAsync(string email, string password, CancellationToken cancellationToken = default);
}
