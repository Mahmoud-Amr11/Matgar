using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.SetDefaultAddress
{
    public class SetDefaultAddressCommandHandler : IRequestHandler<SetDefaultAddressCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public SetDefaultAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var address = await _unitOfWork.Addresses.GetByIdAsync(request.AddressId, cancellationToken);
            if (address is null || address.UserId != userId)
                return Error.NotFound(code: "Address.NotFound", message: "Address not found.");

            if (address.IsDefault)
                return Result.Success;

            var userAddresses = await _unitOfWork.Addresses.FindAsync(
                a => a.UserId == userId, cancellationToken);

            foreach (var userAddress in userAddresses)
            {
                if (userAddress.IsDefault)
                {
                    userAddress.IsDefault = false;
                    _unitOfWork.Addresses.Update(userAddress);
                }
            }

            address.IsDefault = true;
            _unitOfWork.Addresses.Update(address);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}