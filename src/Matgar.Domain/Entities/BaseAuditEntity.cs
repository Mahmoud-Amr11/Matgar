namespace Matgar.Domain.Entities
{
    public abstract class BaseAuditEntity
    {

        public DateTime CreatedAt { get; set; }
        public string CreatedBy { get; set; } = string.Empty;
        public DateTime? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; } = string.Empty;

        public DateTime? DeletedAt { get; set; }
        public bool? IsDeleted { get; set; }
        public string? DeletedBy { get; set; } = string.Empty;

    }
}
