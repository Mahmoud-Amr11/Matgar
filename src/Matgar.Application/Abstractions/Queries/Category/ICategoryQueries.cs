using Matgar.Application.Common.Pagination;
using Matgar.Application.Features.Category.Query.Responses;

namespace Matgar.Application.Abstractions.Queries.Category
{
    public interface ICategoryQueries
    {
        Task<PagedResult<CategoryResponse>> GetAllAsync(
       string? search,
       int offset,
       int pageSize,
       int page,
       CancellationToken cancellationToken);
    }
}
