using Matgar.Application.Features.Addresses.Queries.Responses;

namespace Matgar.Application.Abstractions.Queries.Addresses
{
    public interface IAddressQueries
    {
        Task<List<AddressResponse>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}