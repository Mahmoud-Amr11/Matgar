using Matgar.Application.Common.Results;
using Matgar.Application.Features.Addresses.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.Addresses.Queries.GetAddresses
{
    public sealed record GetAddressesQuery : IRequest<Result<IReadOnlyList<AddressResponse>>>;
}