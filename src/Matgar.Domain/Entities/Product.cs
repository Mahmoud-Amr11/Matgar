namespace Matgar.Domain.Entities
{
    public class Product
    {

        public Guid Id { get; set; }
        public Guid VendorId { get; set; }
        public Guid CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public ProductStatus Status { get; set; } = ProductStatus.Draft;
        public bool IsDeleted { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;




        public Category Category { get; set; } = null!;
        public ICollection<ProductVariant> Variants { get; set; } = new List<ProductVariant>();
        public ICollection<ProductReview> Reviews { get; set; } = new List<ProductReview>();
    }
}