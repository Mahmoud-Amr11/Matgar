using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Notifications.Queries.GetNotifications
{
    public sealed record GetNotificationsQuery() : IRequest<Result<IReadOnlyList<NotificationDto>>>;

    public sealed class NotificationDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public bool IsRead { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
