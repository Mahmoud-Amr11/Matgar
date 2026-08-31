using Matgar.Application.Common.Pagination;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Category.Query.Responses;
using MediatR;

namespace Matgar.Application.Features.Category.Query.GetAllCategories
{
    public class GetAllCategoriesQueryHandler : IRequestHandler<GetAllCategoriesQuery, Result<PagedResult<CategoryResponse>>>
    {
        public Task<Result<PagedResult<CategoryResponse>>> Handle(GetAllCategoriesQuery request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}
