using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Notifications.Commands.MarkRead
{
    public sealed record MarkNotificationReadCommand(Guid Id) : IRequest<Result<bool>>;
}
