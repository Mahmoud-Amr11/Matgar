namespace Matgar.Domain.Entities
{
    public class Category : BaseAuditEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;



        public ICollection<Product> Products = new List<Product>();
    }
}
