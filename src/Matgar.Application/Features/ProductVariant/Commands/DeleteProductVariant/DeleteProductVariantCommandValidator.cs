using FluentValidation;

namespace Matgar.Application.Features.ProductVariant.Commands.DeleteProductVariant
{
    public class DeleteProductVariantCommandValidator : AbstractValidator<DeleteProductVariantCommand>
    {
        public DeleteProductVariantCommandValidator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.VariantId).NotEmpty();
        }
    }
}
