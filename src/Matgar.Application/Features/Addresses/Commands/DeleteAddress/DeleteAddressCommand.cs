using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Addresses.Commands.DeleteAddress
{
    public sealed record DeleteAddressCommand(Guid AddressId) : IRequest<Result>;
}