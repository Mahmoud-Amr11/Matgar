using Dapper;
using Matgar.Application.Abstractions.Dapper;
using Matgar.Application.Abstractions.Queries.Notifications;
using Matgar.Application.Features.Notifications.Queries.GetNotifications;

namespace Matgar.Infrastructure.Persistence.Queries.Notifications
{
    internal sealed class NotificationQueries : INotificationQueries
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public NotificationQueries(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public async Task<IReadOnlyList<NotificationDto>> GetNotificationsForUserAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            using var conn = _connectionFactory.CreateConnection();

            const string sql = """
                SELECT Id, Message AS Body, 0 AS IsRead, CreatedAt
                FROM NotificationLogs
                WHERE UserId = @UserId
                ORDER BY CreatedAt DESC
                """;

            var rows = await conn.QueryAsync<NotificationDto>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: cancellationToken));
            return rows.AsList();
        }
    }
}
