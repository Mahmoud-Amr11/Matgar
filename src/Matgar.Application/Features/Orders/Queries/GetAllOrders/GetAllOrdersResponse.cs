namespace Matgar.Application.Features.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersResponse
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
