using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.UpdateAddress
{
    public class UpdateAddressCommandHandler : IRequestHandler<UpdateAddressCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var address = await _unitOfWork.Addresses.GetByIdAsync(request.AddressId, cancellationToken);
            if (address is null || address.UserId != userId)
                return Error.NotFound(code: "Address.NotFound", message: "Address not found.");

            address.FullAddress = request.FullAddress;
            address.City = request.City;
            address.Governorate = request.Governorate;
            address.PhoneNumber = request.PhoneNumber;

            _unitOfWork.Addresses.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}