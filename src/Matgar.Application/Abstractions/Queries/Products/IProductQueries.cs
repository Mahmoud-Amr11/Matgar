using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Application.Abstractions.Queries.Products
{
    public interface IProductQueries
    {
        Task<PagedResult<ProductListItemResponse>> GetAllAsync(
            string? search,
            Guid? categoryId,
            decimal? minPrice,
            decimal? maxPrice,
            ProductStatus? status,
            int offset,
            int pageSize,
            int page,
            CancellationToken cancellationToken);


        Task<ProductDetailsResponse?> GetByIdAsync(
            Guid id,
            Guid? requestingUserId,
            bool isAdmin,
            CancellationToken cancellationToken);
    }
}