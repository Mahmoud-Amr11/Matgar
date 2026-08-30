using FluentValidation;

namespace Matgar.Application.Features.Auth.Commands.RefreshToken
{
    public sealed class RefreshTokenCommandValidator
    : AbstractValidator<RefreshTokenCommand>
    {
        public RefreshTokenCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}
