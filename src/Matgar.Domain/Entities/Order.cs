using Matgar.Domain.Entities.Common;

namespace Matgar.Domain.Entities
{
    public class Order : BaseAuditEntity
    {
        public Guid Id { get; set; }
        public Guid CustomerId { get; set; }
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; } = 0;
        public decimal TotalAmount { get; set; }
        public Guid? CouponId { get; set; }


        public string ShippingAddressSnapshot { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ConfirmedAt { get; set; }
        public DateTime? ShippedAt { get; set; }
        public DateTime? DeliveredAt { get; set; }
        public DateTime? CancelledAt { get; set; }


        public ICollection<OrderItem> Items { get; set; } = new List<OrderItem>();
        public Payment? Payment { get; set; }

        public string Reference => Id.ToString();
        public decimal Total => TotalAmount;
        public bool IsPaid => Payment is not null && Payment.Status == PaymentStatus.Succeeded;

        public void MarkPaid(string transactionReference)
        {
            var payment = EnsurePayment();
            payment.Status = PaymentStatus.Succeeded;
            payment.TransactionReference = transactionReference;
            payment.CompletedAt = DateTime.UtcNow;
        }

        public void MarkRefunded()
        {
            EnsurePayment().Status = PaymentStatus.Refunded;
        }

        public void MarkVoided()
        {
            var payment = EnsurePayment();
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason ??= "Voided";
        }

        public void MarkPaymentFailed(string? reason = null)
        {
            var payment = EnsurePayment();
            payment.Status = PaymentStatus.Failed;
            payment.FailureReason ??= reason ?? "Payment failed";
        }

        private Payment EnsurePayment()
        {
            if (Payment is null)
            {
                Payment = new Payment
                {
                    OrderId = Id,
                    Provider = PaymentProvider.Paymob,
                    Amount = TotalAmount,
                    Status = PaymentStatus.Pending
                };
            }

            return Payment;
        }
    }
}