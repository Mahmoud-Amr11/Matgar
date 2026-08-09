using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class StockItemConfiguration : IEntityTypeConfiguration<StockItem>
    {
        public void Configure(EntityTypeBuilder<StockItem> builder)
        {
            builder.ToTable("StockItems");

            builder.HasKey(s => s.Id);

            builder.Property(s => s.QuantityOnHand)
                .IsRequired();

            builder.Property(s => s.QuantityReserved)
                .IsRequired();



            builder.Property(s => s.RowVersion)
                .IsRowVersion();


            builder.HasIndex(s => s.ProductVariantId)
                .IsUnique();
        }
    }
}
