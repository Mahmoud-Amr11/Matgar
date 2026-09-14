namespace Matgar.Application.Features.Orders.Queries.GetOrderById
{
    public class OrderDetailResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal SubTotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddressSnapshot { get; set; } = string.Empty;
        public List<OrderItemResponse> Items { get; set; } = new List<OrderItemResponse>();
    }

    public class OrderItemResponse
    {
        public Guid ProductVariantId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public string Sku { get; set; } = string.Empty;
    }
}
