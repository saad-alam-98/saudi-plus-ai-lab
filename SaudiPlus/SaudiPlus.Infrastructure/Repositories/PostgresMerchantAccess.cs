using Microsoft.EntityFrameworkCore;
using SaudiPlus.Application.Interfaces;
using SaudiPlus.Infrastructure.Persistence;
namespace SaudiPlus.Infrastructure.Repositories;
public sealed class PostgresMerchantAccess(SaudiPlusDbContext context) : IMerchantAccess
{
    public Task<bool> ExistsAsync(Guid merchantId, CancellationToken cancellationToken = default) =>
        context.Merchants.AsNoTracking().AnyAsync(m => m.Id == merchantId, cancellationToken);
    public Task<bool> IsMemberAsync(Guid userId, Guid merchantId, CancellationToken cancellationToken = default) =>
        context.MerchantMemberships.AsNoTracking().AnyAsync(m => m.UserId == userId && m.MerchantId == merchantId, cancellationToken);
}
