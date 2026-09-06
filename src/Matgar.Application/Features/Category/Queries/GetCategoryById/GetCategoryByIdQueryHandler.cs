using Matgar.Application.Abstractions.Queries.Category;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Category.Query.Responses;
using MediatR;

namespace Matgar.Application.Features.Category.Query.GetCategoryById;

public sealed class GetCategoryByIdQueryHandler
    : IRequestHandler<GetCategoryByIdQuery, Result<CategoryResponse>>
{
    private readonly ICategoryQueries _categoryQueries;

    public GetCategoryByIdQueryHandler(ICategoryQueries categoryQueries)
    {
        _categoryQueries = categoryQueries;
    }

    public async Task<Result<CategoryResponse>> Handle(GetCategoryByIdQuery request, CancellationToken cancellationToken)
    {
        var category = await _categoryQueries.GetById(request.Id, cancellationToken);

        if (category is null)
            return Error.NotFound(message: "Category not found");

        return category;
    }
}

