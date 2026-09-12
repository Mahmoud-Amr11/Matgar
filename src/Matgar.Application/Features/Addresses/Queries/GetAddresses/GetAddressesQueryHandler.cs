using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Queries.Addresses;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Addresses.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.Addresses.Queries.GetAddresses
{
    public class GetAddressesQueryHandler : IRequestHandler<GetAddressesQuery, Result<IReadOnlyList<AddressResponse>>>
    {
        private readonly IAddressQueries _addressQueries;
        private readonly ICurrentUserService _currentUser;

        public GetAddressesQueryHandler(IAddressQueries addressQueries, ICurrentUserService currentUser)
        {
            _addressQueries = addressQueries;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<AddressResponse>>> Handle(GetAddressesQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var addresses = await _addressQueries.GetByUserIdAsync(userId, cancellationToken);

            return addresses;
        }
    }
}