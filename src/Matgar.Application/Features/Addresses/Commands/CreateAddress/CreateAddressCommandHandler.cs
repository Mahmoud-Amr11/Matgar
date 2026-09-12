using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.CreateAddress
{
    public class CreateAddressCommandHandler : IRequestHandler<CreateAddressCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreateAddressCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            const int maxAddressesPerUser = 20;

            var existingAddresses = await _unitOfWork.Addresses.FindAsync(
                a => a.UserId == userId, cancellationToken);

            if (existingAddresses.Count >= maxAddressesPerUser)
            {
                return Error.Conflict(
                    code: "Address.LimitReached",
                    message: $"You cannot save more than {maxAddressesPerUser} addresses.");
            }

            var isFirstAddress = existingAddresses.Count == 0;
            var makeDefault = request.IsDefault || isFirstAddress;

            var address = new Domain.Entities.Address
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                FullAddress = request.FullAddress,
                City = request.City,
                Governorate = request.Governorate,
                PhoneNumber = request.PhoneNumber,
                IsDefault = makeDefault
            };

            var currentDefaults = existingAddresses.Where(a => a.IsDefault).ToList();
            foreach (var currentDefault in currentDefaults)
            {
                currentDefault.IsDefault = false;
                _unitOfWork.Addresses.Update(currentDefault);
            }

            await _unitOfWork.Addresses.AddAsync(address, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return address.Id;
        }
    }
}