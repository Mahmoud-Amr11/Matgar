using System.ComponentModel.DataAnnotations.Schema;

namespace Matgar.Domain.Entities
{
    public class Coupon : BaseAuditEntity
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int MaxUsageCount { get; set; }
        public int UsedCount { get; set; } = 0;
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; } = true;

        // Helper
        [NotMapped]
        public bool IsValid =>
            IsActive &&
            DateTime.UtcNow < ExpiryDate &&
            UsedCount < MaxUsageCount;
    }
}