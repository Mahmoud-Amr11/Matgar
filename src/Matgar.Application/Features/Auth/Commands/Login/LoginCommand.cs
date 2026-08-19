using Matgar.Application.Common.Results;
using Matgar.Application.Features.Auth.Responses;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Login
{
    public sealed record LoginCommand(string Email, string Password) : IRequest<Result<AuthResponse>>;

}
