using FluentValidation;

namespace Matgar.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandValidator : AbstractValidator<ChangePasswordCommand>
    {

        public ChangePasswordCommandValidator()
        {
            RuleFor(c => c.CurrentPassword).NotEmpty()
                .WithMessage("Old password cant be Empty");

            RuleFor(c => c.NewPassword).NotEmpty()
                .MinimumLength(8);

            RuleFor(c => c.ConfirmPassword).NotEmpty()
                .Matches(c => c.NewPassword);

        }

    }
}
