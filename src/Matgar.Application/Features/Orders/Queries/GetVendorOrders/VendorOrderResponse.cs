namespace Matgar.Application.Features.Orders.Queries.GetVendorOrders
{
    public class VendorOrderResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
