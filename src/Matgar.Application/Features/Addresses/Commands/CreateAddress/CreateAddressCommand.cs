using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.CreateAddress
{
    public sealed record CreateAddressCommand(
        string FullAddress,
        string City,
        string Governorate,
        string PhoneNumber,
        bool IsDefault) : IRequest<Result<Guid>>;
}