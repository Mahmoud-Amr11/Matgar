using FluentValidation;

namespace Matgar.Application.Features.Auth.Commands.Login
{
    public class LoginCommandValidator : AbstractValidator<LoginCommand>
    {

        public LoginCommandValidator()
        {
            RuleFor(l => l.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.");


            RuleFor(l => l.Password)
                .NotEmpty().WithMessage("Password is required.");

        }
    }

}
