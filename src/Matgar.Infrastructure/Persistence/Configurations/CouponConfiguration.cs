using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class CouponConfiguration : IEntityTypeConfiguration<Coupon>
    {
        public void Configure(EntityTypeBuilder<Coupon> builder)
        {
            builder.ToTable("Coupons");

            builder.HasKey(c => c.Id);

            builder.Property(c => c.Code)
                .IsRequired()
                .HasMaxLength(50);

            builder.HasIndex(c => c.Code)
                .IsUnique();

            builder.Property(c => c.DiscountType)
                .HasConversion<string>()
                .HasMaxLength(20)
                .IsRequired();

            builder.Property(c => c.DiscountValue)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(c => c.MinOrderAmount)
                .HasColumnType("decimal(18,2)");

            builder.Property(c => c.ExpiryDate)
                .IsRequired();


        }
    }
}
