namespace SaudiPlus.Application.Interfaces;
public interface ICurrentUser
{
    Guid? UserId { get; }
    bool IsInRole(string role);
}
public interface IMerchantAccess
{
    Task<bool> ExistsAsync(Guid merchantId, CancellationToken cancellationToken = default);
    Task<bool> IsMemberAsync(Guid userId, Guid merchantId, CancellationToken cancellationToken = default);
}
