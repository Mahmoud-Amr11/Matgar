using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.UpdateAddress
{
    public sealed record UpdateAddressCommand(
        Guid AddressId,
        string FullAddress,
        string City,
        string Governorate,
        string PhoneNumber) : IRequest<Result>;
}