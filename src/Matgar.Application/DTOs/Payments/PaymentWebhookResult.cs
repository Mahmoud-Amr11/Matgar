namespace Matgar.Application.DTOs.Payments
{
    public sealed class PaymentWebhookResult
    {
        /// <summary>Paymob transaction id. Unique per transaction.</summary>
        public long TransactionId { get; init; }

        public long ProviderOrderId { get; init; }

        /// <summary>The special_reference we sent when creating the intention (our order reference).</summary>
        public string OrderReference { get; init; } = string.Empty;

        /// <summary>In cents (minor units).</summary>
        public long AmountCents { get; init; }

        public string Currency { get; init; } = string.Empty;

        public bool Success { get; init; }
        public bool Pending { get; init; }
        public bool IsRefunded { get; init; }
        public bool IsVoided { get; init; }
        public bool IsAuth { get; init; }
        public bool IsCapture { get; init; }
    }
}
