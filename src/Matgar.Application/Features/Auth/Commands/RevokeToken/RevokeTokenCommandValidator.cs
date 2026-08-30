using FluentValidation;

namespace Matgar.Application.Features.Auth.Commands.RevokeToken
{
    public sealed class RevokeTokenCommandValidator
    : AbstractValidator<RevokeTokenCommand>
    {
        public RevokeTokenCommandValidator()
        {
            RuleFor(x => x.Token)
                .NotEmpty();
        }
    }
}
