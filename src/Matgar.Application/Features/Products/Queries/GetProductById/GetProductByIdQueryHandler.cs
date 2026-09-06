using Matgar.Application.Abstractions.Queries.Products;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Products.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.Products.Queries.GetProductById
{
    public class GetProductByIdQueryHandler
        : IRequestHandler<GetProductByIdQuery, Result<ProductDetailsResponse>>
    {
        private readonly IProductQueries _productQueries;

        public GetProductByIdQueryHandler(IProductQueries productQueries)
        {
            _productQueries = productQueries;
        }

        public async Task<Result<ProductDetailsResponse>> Handle(
            GetProductByIdQuery request, CancellationToken cancellationToken)
        {
            var product = await _productQueries.GetByIdAsync(request.ProductId, cancellationToken);

            if (product is null)
                return Error.NotFound(
                    code: "Product.NotFound",
                    message: "Product not found.");

            return product;
        }
    }
}
