using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class ProductReviewConfiguration : IEntityTypeConfiguration<ProductReview>
    {
        public void Configure(EntityTypeBuilder<ProductReview> builder)
        {
            builder.ToTable("ProductReviews");

            builder.HasKey(r => r.Id);

            builder.Property(r => r.Rating)
                .IsRequired();

            builder.Property(r => r.Comment)
                .HasMaxLength(2000);

            builder.Property(r => r.CreatedAt)
                .IsRequired();

            
            builder.HasIndex(r => new { r.OrderId, r.ProductId, r.UserId })
                .IsUnique();

            builder.HasOne(r => r.Product)
                .WithMany(p => p.Reviews)
                .HasForeignKey(r => r.ProductId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(r => r.Order)
                .WithMany() 
                .HasForeignKey(r => r.OrderId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
