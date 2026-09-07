using FluentValidation;

namespace Matgar.Application.Features.Cart.Commands.AddCartItem
{
    public class AddCartItemCommandValidator : AbstractValidator<AddCartItemCommand>
    {
        public AddCartItemCommandValidator()
        {
            RuleFor(c => c.ProductVariantId)
                .NotEmpty().WithMessage("ProductVariantId is required.");

            RuleFor(c => c.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100.");
        }
    }
}
