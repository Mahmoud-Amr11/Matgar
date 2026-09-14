

namespace Matgar.Application.Features.Coupons.Queries.GetAllCoupons
{
    public class CouponResponse
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public DiscountType DiscountType { get; set; }
        public decimal DiscountValue { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int MaxUsageCount { get; set; }
        public int UsedCount { get; set; }
        public DateTime ExpiryDate { get; set; }
        public bool IsActive { get; set; }
    }
}
