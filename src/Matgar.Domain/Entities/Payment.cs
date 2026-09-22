using Matgar.Domain.Entities.Common;

namespace Matgar.Domain.Entities
{
    public class Payment : BaseAuditEntity
    {
        public Guid Id { get; set; }
        public Guid OrderId { get; set; }
        public PaymentProvider Provider { get; set; }
        public decimal Amount { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public string Currency { get; set; } = "EGP";
        public string? IdempotencyKey { get; set; }
        public string? TransactionReference { get; set; }
        public string? ProviderPaymentId { get; set; }

        public long? ProviderOrderId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? CompletedAt { get; set; }


        public Order Order { get; set; } = null!;
    }
}