using FluentValidation;

namespace Matgar.Application.Features.Category.Commands.CreateCategory
{
    public class CreateCategoryCommandValidator : AbstractValidator<CreateCategoryCommand>
    {
        public CreateCategoryCommandValidator()
        {
            RuleFor(c => c.Name)
            .NotEmpty()
                .WithMessage("Category name cannot be empty.")
            .MinimumLength(3)
                .WithMessage("Category name must be at least 3 characters.")
            .MaximumLength(100)
                .WithMessage("Category name cannot exceed 100 characters.")
            .Matches(@"^[a-zA-Z0-9\s&'-]+$")
                .WithMessage("Category name contains invalid characters.");
        }
    }
}
