namespace Matgar.Application.Features.Payments.PaymentDtos
{
    public sealed class PaymentItem
    {
        public string Name { get; init; } = string.Empty;

        public decimal Amount { get; init; }

        public string Description { get; init; } = string.Empty;

        public int Quantity { get; init; }
    }
}
