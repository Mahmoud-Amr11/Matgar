using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.RevokeToken
{
    public sealed record RevokeTokenCommand(string? Token) : IRequest<Result>;
}
