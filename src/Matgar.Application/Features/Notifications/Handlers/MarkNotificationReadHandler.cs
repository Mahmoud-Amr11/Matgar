using Matgar.Application.Common.Results;
using Matgar.Application.Features.Notifications.Commands.MarkRead;
using MediatR;
using Matgar.Application.Abstractions.Persistence.Repositories;

namespace Matgar.Application.Features.Notifications.Handlers
{
    public class MarkNotificationReadHandler : IRequestHandler<MarkNotificationReadCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MarkNotificationReadHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(MarkNotificationReadCommand request, CancellationToken cancellationToken)
        {
            // GenericRepository provides GetByIdAsync
            var notif = await _unitOfWork.Notifications.GetByIdAsync(request.Id, cancellationToken);
            if (notif == null) return Error.NotFound(message: "Notification not found.");

            notif.IsRead = true;
            _unitOfWork.Notifications.Update(notif);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
