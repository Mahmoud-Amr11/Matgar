using FluentValidation;

namespace Matgar.Application.Features.Cart.Commands.UpdateCartItem
{
    public class UpdateCartItemCommandValidator : AbstractValidator<UpdateCartItemCommand>
    {
        public UpdateCartItemCommandValidator()
        {
            RuleFor(c => c.CartItemId)
                .NotEmpty().WithMessage("CartItemId is required.");

            RuleFor(c => c.Quantity)
                .GreaterThan(0).WithMessage("Quantity must be greater than zero.")
                .LessThanOrEqualTo(100).WithMessage("Quantity cannot exceed 100.");
        }
    }
}
