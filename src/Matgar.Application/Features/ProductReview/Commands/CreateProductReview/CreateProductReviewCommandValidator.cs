using FluentValidation;

namespace Matgar.Application.Features.ProductReview.Commands.CreateProductReview
{
    public class CreateProductReviewCommandValidator : AbstractValidator<CreateProductReviewCommand>
    {
        public CreateProductReviewCommandValidator()
        {
            RuleFor(c => c.ProductId)
                .NotEmpty();

            RuleFor(c => c.Rating)
                .InclusiveBetween(1, 5)
                .WithMessage("Rating must be between 1 and 5.");

            RuleFor(c => c.Comment)
                .MaximumLength(2000)
                .WithMessage("Comment cannot exceed 2000 characters.");
        }
    }
}
