using FluentValidation;

namespace Matgar.Application.Features.Products.Commands.UpdateProduct
{
    public class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductCommandValidator()
        {
            RuleFor(c => c.ProductId).NotEmpty();

            RuleFor(c => c.Name)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("Product name is required.")
                .MinimumLength(3).WithMessage("Product name must be at least 3 characters.")
                .MaximumLength(250).WithMessage("Product name cannot exceed 250 characters.");

            RuleFor(c => c.Description)
                .MaximumLength(4000).WithMessage("Description cannot exceed 4000 characters.");

            RuleFor(c => c.CategoryId)
                .NotEmpty().WithMessage("CategoryId is required.");
        }
    }
}
