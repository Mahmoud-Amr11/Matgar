using Matgar.Application.Abstractions.Identity;
using Matgar.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Text.Json;

namespace Matgar.Infrastructure.Persistence.Interceptor
{
    internal class AuditSaveChangesInterceptor(ICurrentUserService _currentUserService) : SaveChangesInterceptor
    {
        public override InterceptionResult<int> SavingChanges(DbContextEventData eventData, InterceptionResult<int> result)
        {
            ApplyAuditRules(eventData.Context);
            return base.SavingChanges(eventData, result);
        }
        public override ValueTask<InterceptionResult<int>> SavingChangesAsync(DbContextEventData eventData, InterceptionResult<int> result, CancellationToken cancellationToken = default)
        {
            ApplyAuditRules(eventData.Context);
            return base.SavingChangesAsync(eventData, result, cancellationToken);
        }

        private void ApplyAuditRules(DbContext? context)
        {
            if (context is null)
                return;

            var now = DateTime.UtcNow;
            var userId = string.IsNullOrWhiteSpace(_currentUserService.UserId)
                    ? "System"
                    : _currentUserService.UserId;

            SetAuditFields(context, userId, now);


            var auditLogs = BuildEntityLog(context, userId, now);

            if (auditLogs.Count > 0)
                context.Set<AuditLog>().AddRange(auditLogs);
        }

        private static void SetAuditFields(DbContext context, string currentUserId, DateTime now)
        {

            foreach (var entry in context.ChangeTracker.Entries<BaseAuditEntity>())
            {
                if (entry.State == EntityState.Added)
                {
                    entry.Entity.CreatedAt = now;
                    entry.Entity.CreatedBy = currentUserId;
                }
                if (entry.State == EntityState.Modified)
                {
                    entry.Entity.UpdatedAt = now;
                    entry.Entity.UpdatedBy = currentUserId;
                }
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entry.Entity.IsDeleted = true;
                    entry.Entity.DeletedBy = currentUserId;
                    entry.Entity.DeletedAt = now;
                }
            }
        }


        private static List<AuditLog> BuildEntityLog(DbContext context, string currentUserId, DateTime now)
        {
            var logs = new List<AuditLog>();
            foreach (var entry in context.ChangeTracker.Entries())
            {
                if (entry.Entity is AuditLog)
                    continue;

                if (entry.State is not (EntityState.Added or EntityState.Modified or EntityState.Deleted))
                    continue;
                var changes = BuildChangesDictionary(entry);
                if (changes.Count == 0)
                    continue;

                logs.Add(new AuditLog
                {
                    EntityName = entry.Entity.GetType().Name,
                    EntityId = GetPrimaryKeyValue(entry),
                    Action = entry.State.ToString(),

                    ChangedAt = now,
                    ChangedBy = currentUserId,
                    Changes = JsonSerializer.Serialize(changes)
                });
            }

            return logs;
        }
        private static Dictionary<string, object> BuildChangesDictionary(EntityEntry entity)
        {
            var changes = new Dictionary<string, object>();

            foreach (var property in entity.Properties)
            {
                switch (entity.State)
                {
                    case EntityState.Added:
                        changes[entity.Metadata.Name] = property.CurrentValue;
                        break;

                    case EntityState.Deleted:
                        changes[entity.Metadata.Name] = property.OriginalValue;
                        break;

                    case EntityState.Modified when property.IsModified:
                        changes[entity.Metadata.Name] = new
                        {
                            oldValue = property.OriginalValue,
                            newValue = property.CurrentValue
                        };
                        break;
                }
            }
            return changes;
        }

        private static string? GetPrimaryKeyValue(EntityEntry entry)
        {
            var key = entry.Metadata.FindPrimaryKey();

            if (key is null) return string.Empty;


            var values = key.Properties.Select(prop => entry.Property(prop.Name).CurrentValue?.ToString())
                .Where(value => value is not null);


            return string.Join(",", values);
        }

    }
}
