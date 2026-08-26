using FluentValidation;

namespace Matgar.Application.Features.Category.Commands.DeleteCategory
{
    public class DeleteCategoryCommandValidator : AbstractValidator<DeleteCategoryCommand>
    {
        public DeleteCategoryCommandValidator()
        {
            RuleFor(c => c.CategoryId)
                .NotEmpty()
                .WithMessage("Please enter the category id");
        }

    }

}
