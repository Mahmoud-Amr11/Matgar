using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Coupons.Queries.GetAllCoupons
{
    public class GetAllCouponsQueryHandler : IRequestHandler<GetAllCouponsQuery, Result<IReadOnlyList<CouponResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllCouponsQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<CouponResponse>>> Handle(GetAllCouponsQuery request, CancellationToken cancellationToken)
        {
            var coupons = await _unitOfWork.Coupons.GetAllAsync(cancellationToken);

            var response = coupons.Select(c => new CouponResponse
            {
                Id = c.Id,
                Code = c.Code,
                DiscountType = c.DiscountType,
                DiscountValue = c.DiscountValue,
                MinOrderAmount = c.MinOrderAmount,
                MaxUsageCount = c.MaxUsageCount,
                UsedCount = c.UsedCount,
                ExpiryDate = c.ExpiryDate,
                IsActive = c.IsActive
            }).ToList();

            return Result<IReadOnlyList<CouponResponse>>.Success(response);
        }
    }
}
