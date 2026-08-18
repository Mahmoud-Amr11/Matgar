using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.ConfirmEmail
{
    public sealed record ConfirmEmailCommand(string UserId, string EncodedToken) : IRequest<Result>;
}
