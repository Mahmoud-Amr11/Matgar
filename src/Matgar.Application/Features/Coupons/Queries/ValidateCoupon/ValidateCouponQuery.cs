using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Coupons.Queries.ValidateCoupon
{
    public sealed record ValidateCouponQuery(string Code, decimal OrderTotal) : IRequest<Result<ValidateCouponResponse>>;
}
