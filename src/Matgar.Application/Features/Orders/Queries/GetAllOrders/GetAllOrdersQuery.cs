using MediatR;
using Matgar.Application.Common.Results;
using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Pagination;

namespace Matgar.Application.Features.Orders.Queries.GetAllOrders
{
    public sealed record GetAllOrdersQuery(int Page = 1, int PageSize = 20) : ICacheableQuery<PagedResult<GetAllOrdersResponse>>
    {
        public string CacheKey => $"GetAllOrders_Page_{Page}_Size_{PageSize}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
        public bool BypassCache => false;
    };
}
