using FluentValidation;

namespace Matgar.Application.Features.Addresses.Commands.UpdateAddress
{
    public class UpdateAddressCommandValidator : AbstractValidator<UpdateAddressCommand>
    {
        public UpdateAddressCommandValidator()
        {
            RuleFor(c => c.AddressId)
                .NotEmpty().WithMessage("AddressId is required.");

            RuleFor(c => c.FullAddress)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("FullAddress is required.")
                .MaximumLength(500).WithMessage("FullAddress cannot exceed 500 characters.");

            RuleFor(c => c.City)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("City is required.")
                .MaximumLength(100).WithMessage("City cannot exceed 100 characters.");

            RuleFor(c => c.Governorate)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Governorate is required.")
                .MaximumLength(100).WithMessage("Governorate cannot exceed 100 characters.");

            RuleFor(c => c.PhoneNumber)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("PhoneNumber is required.")
                .MaximumLength(20).WithMessage("PhoneNumber cannot exceed 20 characters.")
                .Matches(@"^\+?[0-9\s\-()]{7,20}$").WithMessage("PhoneNumber is not in a valid format.");
        }
    }
}