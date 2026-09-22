using Matgar.Application.Features.Notifications.Queries.GetNotifications;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Abstractions.Persistence.Queries.Notifications
{
    public interface INotificationQueries
    {
        Task<IReadOnlyList<NotificationDto>> GetNotificationsForUserAsync(Guid userId, CancellationToken cancellationToken = default);
    }
}
