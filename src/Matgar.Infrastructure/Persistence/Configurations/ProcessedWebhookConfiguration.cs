using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class ProcessedWebhookConfiguration : IEntityTypeConfiguration<ProcessedWebhook>
    {
        public void Configure(EntityTypeBuilder<ProcessedWebhook> builder)
        {
            builder.ToTable("ProcessedWebhooks");

            builder.HasKey(p => p.Id);

            builder.Property(p => p.EventKey)
                .HasMaxLength(300)
                .IsRequired();

            builder.HasIndex(p => p.EventKey)
                .IsUnique();

            builder.Property(p => p.ProcessedAt)
                .IsRequired();
        }
    }
}