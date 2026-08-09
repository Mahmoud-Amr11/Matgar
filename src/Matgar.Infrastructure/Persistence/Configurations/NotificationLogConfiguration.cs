using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Matgar.Infrastructure.Persistence.Configurations
{
    public class NotificationLogConfiguration : IEntityTypeConfiguration<NotificationLog>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<NotificationLog> builder)
        {
            builder.ToTable("NotificationLogs");

            builder.HasKey(n => n.Id);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(1000);

            builder.Property(n => n.CreatedAt)
                .IsRequired();

            builder.HasIndex(n => n.UserId);


            builder.Property(n => n.RelatedOrderId)
                .IsRequired(false);

        }
    }
}
