using Matgar.Domain;

namespace Matgar.Application.Features.Orders.Queries.GetOrders
{
    public class OrderResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
