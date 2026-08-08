namespace Matgar.Domain.Entities
{
    public class ProductVariant
    {
        public Guid Id { get; set; }
        public Guid ProductId { get; set; }
        public string Sku { get; set; } = string.Empty;
        public string AttributesJson { get; set; } = "{}";
        public decimal Price { get; set; }
        public string? ImageUrl { get; set; }
        public bool IsDeleted { get; set; } = false;

        // Navigation
        public Product Product { get; set; } = null!;
        public StockItem? StockItem { get; set; }

    }
}