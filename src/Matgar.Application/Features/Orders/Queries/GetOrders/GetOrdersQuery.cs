using MediatR;
using Matgar.Application.Common.Results;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Abstractions.Caching;

namespace Matgar.Application.Features.Orders.Queries.GetOrders
{
    public sealed record GetOrdersQuery(int Page = 1, int PageSize = 20) : ICacheableQuery<PagedResult<OrderResponse>>
    {
        public string CacheKey => $"GetOrders_User_Page_{Page}_Size_{PageSize}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
        public bool BypassCache => true;
    };
}
