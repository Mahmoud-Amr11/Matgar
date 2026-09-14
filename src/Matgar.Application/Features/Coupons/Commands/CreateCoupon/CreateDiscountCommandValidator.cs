using FluentValidation;

namespace Matgar.Application.Features.Coupons.Commands.CreateCoupon
{
    public class CreateDiscountCommandValidator
        : AbstractValidator<CreateDiscountCommand>
    {
        public CreateDiscountCommandValidator()
        {
            RuleFor(x => x.Code)
                .NotEmpty()
                .WithMessage("Coupon code is required.")
                .MaximumLength(50)
                .WithMessage("Coupon code cannot exceed 50 characters.");

            RuleFor(x => x.DiscountType)
                .IsInEnum()
                .WithMessage("Invalid discount type.");

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0)
                .WithMessage("Discount value must be greater than 0.");

            RuleFor(x => x.MinOrderAmount)
                .GreaterThanOrEqualTo(0)
                .When(x => x.MinOrderAmount.HasValue)
                .WithMessage("Minimum order amount cannot be negative.");

            RuleFor(x => x.MaxUsageCount)
                .GreaterThan(0)
                .WithMessage("Maximum usage count must be greater than 0.");

            RuleFor(x => x.ExpiryDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("Expiry date must be in the future.");
        }
    }
}