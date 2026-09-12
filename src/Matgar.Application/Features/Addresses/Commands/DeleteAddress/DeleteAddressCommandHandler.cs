using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.DeleteAddress
{
    public class DeleteAddressCommandHandler : IRequestHandler<DeleteAddressCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public DeleteAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var address = await _unitOfWork.Addresses.GetByIdAsync(request.AddressId, cancellationToken);
            if (address is null || address.UserId != userId)
                return Error.NotFound(code: "Address.NotFound", message: "Address not found.");

            var wasDefault = address.IsDefault;

            _unitOfWork.Addresses.Remove(address);

            if (wasDefault)
            {
                var remainingAddresses = await _unitOfWork.Addresses.FindAsync(
                    a => a.UserId == userId && a.Id != address.Id, cancellationToken);

                var nextDefault = remainingAddresses
                    .OrderByDescending(a => a.CreatedAt)
                    .FirstOrDefault();

                if (nextDefault is not null)
                {
                    nextDefault.IsDefault = true;
                    _unitOfWork.Addresses.Update(nextDefault);
                }
            }

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}