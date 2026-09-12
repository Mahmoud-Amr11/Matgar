using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.SetDefaultAddress
{
    public sealed record SetDefaultAddressCommand(Guid AddressId) : IRequest<Result>;
}