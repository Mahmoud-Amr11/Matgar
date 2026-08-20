using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.ChangePassword
{
    public sealed record ChangePasswordCommand(string CurrentPassword, string NewPassword, string ConfirmPassword) : IRequest<Result>;
}
