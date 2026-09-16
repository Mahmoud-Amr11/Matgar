using Matgar.Application.Common.Results;
using Matgar.Application.Features.Notifications.Queries.GetNotifications;
using Matgar.Application.Abstractions.Queries.Notifications;
using MediatR;
using Matgar.Application.Abstractions.Identity;

namespace Matgar.Application.Features.Notifications.Handlers
{
    public class GetNotificationsHandler : IRequestHandler<GetNotificationsQuery, Result<IReadOnlyList<NotificationDto>>>
    {
        private readonly INotificationQueries _queries;
        private readonly ICurrentUserService _currentUser;

        public GetNotificationsHandler(INotificationQueries queries, ICurrentUserService currentUser)
        {
            _queries = queries;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<NotificationDto>>> Handle(GetNotificationsQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var list = await _queries.GetNotificationsForUserAsync(userId, cancellationToken);
            return Result<IReadOnlyList<NotificationDto>>.Success(list);
        }
    }
}
