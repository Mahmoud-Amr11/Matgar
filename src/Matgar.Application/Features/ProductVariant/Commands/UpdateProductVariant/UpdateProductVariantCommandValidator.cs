using FluentValidation;
using System.Text.Json;

namespace Matgar.Application.Features.ProductVariant.Commands.UpdateProductVariant
{
    public class UpdateProductVariantCommandValidator : AbstractValidator<UpdateProductVariantCommand>
    {
        public UpdateProductVariantCommandValidator()
        {
            RuleFor(c => c.ProductId).NotEmpty();
            RuleFor(c => c.VariantId).NotEmpty();

            RuleFor(c => c.Price)
                .GreaterThan(0).WithMessage("Price must be greater than zero.")
                .LessThanOrEqualTo(1_000_000).WithMessage("Price seems unreasonably high.");

            RuleFor(c => c.ImageUrl)
                .MaximumLength(500).WithMessage("ImageUrl cannot exceed 500 characters.")
                .Must(url => string.IsNullOrEmpty(url) || Uri.TryCreate(url, UriKind.Absolute, out _))
                .WithMessage("ImageUrl must be a valid absolute URL.");

            RuleFor(c => c.AttributesJson)
                .NotEmpty().WithMessage("AttributesJson is required.")
                .Must(BeValidJson).WithMessage("AttributesJson must be valid JSON.");
        }

        private static bool BeValidJson(string json)
        {
            try
            {
                using var _ = JsonDocument.Parse(json);
                return true;
            }
            catch (System.Text.Json.JsonException)
            {
                return false;
            }
        }
    }
}
