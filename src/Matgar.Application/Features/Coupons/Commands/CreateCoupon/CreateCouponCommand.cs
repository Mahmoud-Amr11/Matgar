using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Coupons.Commands.CreateCoupon
{
    public record CreateDiscountCommand(
    string Code,
    DiscountType DiscountType,
    decimal DiscountValue,
    decimal? MinOrderAmount,
    int MaxUsageCount,
    DateTime ExpiryDate
                        ) : IRequest<Result<Guid>>;
}
