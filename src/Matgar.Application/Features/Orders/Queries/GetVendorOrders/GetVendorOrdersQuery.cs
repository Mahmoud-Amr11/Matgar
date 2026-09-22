using MediatR;
using Matgar.Application.Common.Results;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Abstractions.Caching;

namespace Matgar.Application.Features.Orders.Queries.GetVendorOrders
{
    public sealed record GetVendorOrdersQuery(int Page = 1, int PageSize = 20) : ICacheableQuery<PagedResult<VendorOrderResponse>>
    {
        public string CacheKey => $"GetVendorOrders_Vendor_Page_{Page}_Size_{PageSize}";
        public TimeSpan? Expiration => TimeSpan.FromMinutes(2);
        public bool BypassCache => true;
    };
}
