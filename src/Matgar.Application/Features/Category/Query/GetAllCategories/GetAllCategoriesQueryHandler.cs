using Matgar.Application.Abstractions.Queries.Category;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Category.Query.Responses;
using MediatR;

namespace Matgar.Application.Features.Category.Query.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<PagedResult<CategoryResponse>>>
    {
        private readonly ICategoryQueries _categoryQueries;

        public GetAllCategoriesQueryHandler(ICategoryQueries categoryQueries)
        {
            _categoryQueries = categoryQueries;
        }

        public async Task<Result<PagedResult<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            var pagination = new PaginationParams
            {
                Page = request.Page,
                PageSize = request.PageSize
            };

            var result = await _categoryQueries.GetAllAsync(
                request.Search,
                pagination.Offset,
                pagination.NormalizedPageSize,
                pagination.NormalizedPage,
                cancellationToken);



            return result;
        }
    }
}
