using FluentValidation;
using System.Text.Json;

namespace Matgar.Application.Features.ProductVariant.Commands.CreateProductVariant
{
    public class CreateProductVariantCommandValidator : AbstractValidator<CreateProductVariantCommand>
    {
        public CreateProductVariantCommandValidator()
        {
            RuleFor(c => c.ProductId).NotEmpty();

            RuleFor(c => c.Sku)
                .Cascade(CascadeMode.Stop)
                .NotEmpty().WithMessage("SKU is required.")
                .MaximumLength(100).WithMessage("SKU cannot exceed 100 characters.")
                .Matches(@"^[A-Za-z0-9\-_]+$").WithMessage("SKU can only contain letters, numbers, hyphens, and underscores.");

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

            RuleFor(c => c.InitialQuantity)
                .GreaterThanOrEqualTo(0).WithMessage("InitialQuantity cannot be negative.");
        }

        // بنتحقق إن الـ AttributesJson فعلًا JSON صحيح قبل ما يوصل لقاعدة
        // البيانات -- من غير الفحص ده، أي نص عشوائي هيتخزن كـ "attributes"
        // ويبوظ أي كود بعد كده يحاول يعمل Deserialize له.
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
