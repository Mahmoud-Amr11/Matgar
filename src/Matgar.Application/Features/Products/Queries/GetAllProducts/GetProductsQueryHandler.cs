using Matgar.Application.Abstractions.Queries.Products;
using Matgar.Application.Common.Pagination;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Products.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.Products.Queries.GetAllProducts
{
    public class GetProductsQueryHandler
        : IRequestHandler<GetProductsQuery, Result<PagedResult<ProductListItemResponse>>>
    {
        private readonly IProductQueries _productQueries;

        public GetProductsQueryHandler(IProductQueries productQueries)
        {
            _productQueries = productQueries;
        }

        public async Task<Result<PagedResult<ProductListItemResponse>>> Handle(
            GetProductsQuery request, CancellationToken cancellationToken)
        {
            var pagination = new PaginationParams
            {
                Page = request.Page,
                PageSize = request.PageSize
            };


            if (request.MinPrice.HasValue && request.MaxPrice.HasValue
                && request.MinPrice > request.MaxPrice)
            {
                return Error.Validation(
                    code: "Product.InvalidPriceRange",
                    message: "MinPrice cannot be greater than MaxPrice.");
            }

            var result = await _productQueries.GetAllAsync(
                request.Search,
                request.CategoryId,
                request.MinPrice,
                request.MaxPrice,
                request.Status,
                pagination.Offset,
                pagination.NormalizedPageSize,
                pagination.NormalizedPage,
                cancellationToken);

            return result;
        }
    }
}
