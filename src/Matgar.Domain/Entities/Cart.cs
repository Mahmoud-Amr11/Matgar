namespace Matgar.Domain.Entities
{
    public class Cart : BaseAuditEntity
    {
        public Guid Id { get; set; }
        public Guid? UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<CartItem> Items { get; set; } = new List<CartItem>();
    }
}