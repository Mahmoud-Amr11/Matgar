using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class ProductVariantConfiguration : IEntityTypeConfiguration<ProductVariant>
    {
        public void Configure(EntityTypeBuilder<ProductVariant> builder)
        {
            builder.ToTable("ProductVariants");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Sku)
                .IsRequired()
                .HasMaxLength(100);


            builder.HasIndex(v => v.Sku)
                .IsUnique();

            builder.Property(v => v.AttributesJson)
                .HasColumnType("nvarchar(max)")
                .IsRequired();

            builder.Property(v => v.Price)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(v => v.ImageUrl)
                .HasMaxLength(500);

            builder.HasQueryFilter(v => !v.IsDeleted);

            builder.HasOne(v => v.Product)
                .WithMany(p => p.Variants)
                .HasForeignKey(v => v.ProductId)
                .OnDelete(DeleteBehavior.Cascade);


            builder.HasOne(v => v.StockItem)
                .WithOne(s => s.ProductVariant)
                .HasForeignKey<StockItem>(s => s.ProductVariantId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
