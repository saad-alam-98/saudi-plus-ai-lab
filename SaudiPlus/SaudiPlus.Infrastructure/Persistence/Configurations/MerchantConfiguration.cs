using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaudiPlus.Domain.Entities;
namespace SaudiPlus.Infrastructure.Persistence.Configurations;
public sealed class MerchantConfiguration : IEntityTypeConfiguration<Merchant>
{
    public void Configure(EntityTypeBuilder<Merchant> builder)
    {
        builder.ToTable("merchants");
        builder.HasKey(m => m.Id);
        builder.Property(m => m.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(m => m.Name).HasColumnName("name").HasMaxLength(200).IsRequired();
    }
}
public sealed class MerchantMembershipConfiguration : IEntityTypeConfiguration<MerchantMembership>
{
    public void Configure(EntityTypeBuilder<MerchantMembership> builder)
    {
        builder.ToTable("merchant_memberships");
        builder.HasKey(m => new { m.UserId, m.MerchantId });
        builder.Property(m => m.UserId).HasColumnName("user_id");
        builder.Property(m => m.MerchantId).HasColumnName("merchant_id");
        builder.HasOne<Merchant>().WithMany().HasForeignKey(m => m.MerchantId);
        builder.HasOne<Microsoft.AspNetCore.Identity.IdentityUser<Guid>>().WithMany().HasForeignKey(m => m.UserId);
    }
}
