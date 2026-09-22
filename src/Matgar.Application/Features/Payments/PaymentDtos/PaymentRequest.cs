namespace Matgar.Application.Features.Payments.PaymentDtos
{
    public sealed class PaymentRequest
    {
        public decimal Amount { get; init; }

        public string Currency { get; init; } = "EGP";

        public string OrderReference { get; init; } = string.Empty;

        public string CustomerFirstName { get; init; } = string.Empty;

        public string CustomerLastName { get; init; } = string.Empty;

        public string CustomerEmail { get; init; } = string.Empty;

        public string CustomerPhone { get; init; } = string.Empty;

        public IReadOnlyCollection<PaymentItem> Items { get; init; }
            = [];
    }
}
