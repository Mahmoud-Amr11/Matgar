using Matgar.Application.Common.Enums;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Register
{
    public sealed record RegisterCommand(string FirstName, string LastName, string Email, string Password, string ConfirmPassword, UserType UserType) : IRequest<Result>;

}
