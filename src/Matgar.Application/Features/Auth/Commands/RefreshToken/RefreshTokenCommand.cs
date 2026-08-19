using Matgar.Application.Common.Results;
using Matgar.Application.Features.Auth.Responses;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.RefreshToken
{
    public sealed record RefreshTokenCommand(string Token) : IRequest<Result<AuthResponse>>;
}
