using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class ProductConfiguration : IEntityTypeConfiguration<Product>
    {
        public void Configure(EntityTypeBuilder<Product> builder)
        {
            builder.ToTable("Products");
            builder.HasKey(p => p.Id);


            builder.Property(p => p.Name)
               .IsRequired()
               .HasMaxLength(250);

            builder.Property(p => p.Description)
                .HasMaxLength(4000);


            builder.Property(p => p.Status)
                .IsRequired()
                .HasConversion<string>();




            builder.HasQueryFilter(p => p.IsDeleted == false);


            builder.HasIndex(p => p.VendorId);
            builder.HasIndex(p => new { p.CategoryId, p.Status });


            builder.HasMany(p => p.Variants)
              .WithOne(v => v.Product)
              .HasForeignKey(v => v.ProductId)
              .OnDelete(DeleteBehavior.Cascade);


            builder.HasMany(p => p.Reviews)
                .WithOne(r => r.Product)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
