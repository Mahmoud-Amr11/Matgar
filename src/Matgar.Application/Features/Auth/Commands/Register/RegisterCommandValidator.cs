using FluentValidation;

namespace Matgar.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
    {
        public RegisterCommandValidator()
        {
            RuleFor(r => r.FirstName)
               .NotEmpty()
               .WithMessage("First name is required.")
               .MinimumLength(3)
               .WithMessage("First name must be at least 3 characters.")
               .MaximumLength(50)
               .WithMessage("First name must not exceed 50 characters.")
               .Matches("^[a-zA-Z]+$")
               .WithMessage("First name can only contain letters.");

            RuleFor(r => r.LastName)
                .NotEmpty()
                .WithMessage("Last name is required.")
                .MinimumLength(3)
                .WithMessage("Last name must be at least 3 characters.")
                .MaximumLength(50)
                .WithMessage("Last name must not exceed 50 characters.")
                .Matches("^[a-zA-Z]+$")
                .WithMessage("Last name can only contain letters.");

            RuleFor(r => r.Email)
                .NotEmpty()
                .WithMessage("Email is required.")
                .MaximumLength(256)
                .WithMessage("Email must not exceed 256 characters.")
                .EmailAddress()
                .WithMessage("Invalid email format.");

            RuleFor(r => r.Password)
                .NotEmpty()
                .WithMessage("Password is required.")
                .MinimumLength(8);


            RuleFor(r => r.ConfirmPassword)
                .NotEmpty()
                .WithMessage("Confirm password is required.")
                .Equal(r => r.Password)
                .WithMessage("Passwords do not match.");

        }
    }

}
