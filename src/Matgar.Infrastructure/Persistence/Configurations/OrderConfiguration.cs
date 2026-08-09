using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(EntityTypeBuilder<Order> builder)
        {
            builder.ToTable("Orders");

            builder.HasKey(o => o.Id);

            builder.Property(o => o.Status)
                .HasConversion<string>()
                .HasMaxLength(30)
                .IsRequired();

            builder.Property(o => o.SubTotal)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.DiscountAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)")
                .IsRequired();


            builder.Property(o => o.ShippingAddressSnapshot)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(o => o.CreatedAt)
                .IsRequired();

            builder.HasIndex(o => o.CustomerId);
            builder.HasIndex(o => o.Status);

            builder.HasMany(o => o.Items)
                .WithOne(i => i.Order)
                .HasForeignKey(i => i.OrderId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(o => o.Payment)
                .WithOne(p => p.Order)
                .HasForeignKey<Payment>(p => p.OrderId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.Property(o => o.CouponId)
                .IsRequired(false);
        }
    }
}
