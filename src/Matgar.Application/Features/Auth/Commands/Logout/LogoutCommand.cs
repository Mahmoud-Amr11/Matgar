using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Logout
{
    public sealed record LogoutCommand() : IRequest<Result>;

}
