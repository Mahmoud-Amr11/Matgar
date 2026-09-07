using FluentValidation;

namespace Matgar.Application.Features.Products.Commands.SuspendProduct
{
    public class SuspendProductCommandValidator : AbstractValidator<SuspendProductCommand>
    {
        public SuspendProductCommandValidator()
        {
            RuleFor(c => c.Reason)
                .NotEmpty().WithMessage("A reason is required to suspend a product.")
                .MaximumLength(500).WithMessage("Reason cannot exceed 500 characters.");
        }
    }
}
