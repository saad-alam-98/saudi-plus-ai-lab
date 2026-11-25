using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SaudiPlus.Domain.Entities;

namespace SaudiPlus.Infrastructure.Persistence.Configurations;

public sealed class OfferConfiguration : IEntityTypeConfiguration<Offer>
{
    public void Configure(EntityTypeBuilder<Offer> builder)
    {
        builder.ToTable("offers");
        builder.HasKey(offer => offer.Id);
        builder.Property(offer => offer.Id).HasColumnName("id").ValueGeneratedNever();
        builder.Property(offer => offer.MerchantId).HasColumnName("merchant_id").IsRequired();
        builder.Property(offer => offer.Title).HasColumnName("title").HasMaxLength(200).IsRequired();
        builder.Property(offer => offer.DiscountPercentage).HasColumnName("discount_percentage").HasPrecision(5, 2).IsRequired();
        builder.Property(offer => offer.StartDate).HasColumnName("start_date").HasColumnType("timestamp with time zone").IsRequired();
        builder.Property(offer => offer.EndDate).HasColumnName("end_date").HasColumnType("timestamp with time zone").IsRequired();
        builder.HasIndex(offer => new { offer.DiscountPercentage, offer.Id })
            .IsDescending(true, false)
            .HasDatabaseName("ix_offers_discount_id");
    }
}
