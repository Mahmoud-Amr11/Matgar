using FluentValidation;

namespace Matgar.Application.Features.Category.Commands.UpdateCategory
{
    public class UpdateCategoryCommandValidator : AbstractValidator<UpdateCategoryCommand>
    {
        public UpdateCategoryCommandValidator()
        {
            RuleFor(c => c.CategoryId)
                .NotEmpty()
                .WithMessage("Please enter the category id");
            RuleFor(c => c.NewName)
                .Cascade(CascadeMode.Stop)
                .NotEmpty()
                .WithMessage("Please enter the new category name")
                .MaximumLength(100)
                .WithMessage("The new category name must not exceed 100 characters")
                 .Matches(@"^[a-zA-Z0-9\s&'-]+$")
                .WithMessage("Category name contains invalid characters."); ;
        }
    }
}
