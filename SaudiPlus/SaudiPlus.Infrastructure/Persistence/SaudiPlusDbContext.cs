using Microsoft.EntityFrameworkCore;
using SaudiPlus.Domain.Entities;

namespace SaudiPlus.Infrastructure.Persistence;

public sealed class SaudiPlusDbContext(DbContextOptions<SaudiPlusDbContext> options) : DbContext(options)
{
    public DbSet<Offer> Offers => Set<Offer>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(SaudiPlusDbContext).Assembly);
    }
}
