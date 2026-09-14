using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Coupons.Queries.ValidateCoupon
{
    public class ValidateCouponQueryHandler : IRequestHandler<ValidateCouponQuery, Result<ValidateCouponResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public ValidateCouponQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<ValidateCouponResponse>> Handle(ValidateCouponQuery request, CancellationToken cancellationToken)
        {
            var code = request.Code?.Trim();
            if (string.IsNullOrWhiteSpace(code))
                return Error.Validation(code: "Coupon.CodeRequired", message: "Coupon code is required.");

            var coupons = await _unitOfWork.Coupons.FindAsync(c => c.Code.ToLower() == code.ToLower(), cancellationToken);
            var coupon = coupons.FirstOrDefault();

            if (coupon == null)
                return Error.NotFound(code: "Coupon.NotFound", message: "Coupon not found.");

            if (!coupon.IsActive)
                return Error.Validation(code: "Coupon.Inactive", message: "Coupon is not active.");

            if (DateTime.UtcNow >= coupon.ExpiryDate)
                return Error.Validation(code: "Coupon.Expired", message: "Coupon has expired.");

            if (coupon.UsedCount >= coupon.MaxUsageCount)
                return Error.Validation(code: "Coupon.UsageExceeded", message: "Coupon usage limit exceeded.");

            if (coupon.MinOrderAmount.HasValue && request.OrderTotal < coupon.MinOrderAmount.Value)
                return Error.Validation(code: "Coupon.MinOrderNotMet", message: "Order total does not meet the minimum required for this coupon.");

            decimal discountAmount = coupon.DiscountType == DiscountType.Percentage
                ? Math.Round(request.OrderTotal * coupon.DiscountValue / 100m, 2)
                : coupon.DiscountValue;

            if (discountAmount > request.OrderTotal) discountAmount = request.OrderTotal;

            var response = new ValidateCouponResponse
            {
                Id = coupon.Id,
                Code = coupon.Code,
                DiscountType = coupon.DiscountType,
                DiscountValue = coupon.DiscountValue,
                DiscountAmount = discountAmount,
                MinOrderAmount = coupon.MinOrderAmount,
                MaxUsageCount = coupon.MaxUsageCount,
                UsedCount = coupon.UsedCount,
                ExpiryDate = coupon.ExpiryDate,
                IsActive = coupon.IsActive
            };

            return Result<ValidateCouponResponse>.Success(response);
        }
    }
}
