using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Coupons.Queries.GetAllCoupons
{
    public sealed record GetAllCouponsQuery() : IRequest<Result<IReadOnlyList<CouponResponse>>>;
}
