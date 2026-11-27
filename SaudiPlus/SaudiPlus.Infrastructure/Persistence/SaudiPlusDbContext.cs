using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SaudiPlus.Domain.Entities;

namespace SaudiPlus.Infrastructure.Persistence;

public sealed class SaudiPlusDbContext(DbContextOptions<SaudiPlusDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    public DbSet<Offer> Offers => Set<Offer>();
    public DbSet<Merchant> Merchants => Set<Merchant>();
    public DbSet<MerchantMembership> MerchantMemberships => Set<MerchantMembership>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaudiPlusDbContext).Assembly);
    }
}
