namespace Matgar.Application.Features.Payments.PaymentDtos
{
    public sealed class PaymentResult
    {
        public string IntentionId { get; init; } = string.Empty;

        public long PaymobOrderId { get; init; }

        public string ClientSecret { get; init; } = string.Empty;

        public string CheckoutUrl { get; init; } = string.Empty;
    }
}
