using Matgar.Application.Abstractions.Queries.ProductVariant;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Products.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.ProductVariant.Queries.GetProductVariants
{
    public class GetProductVariantsQueryHandler
       : IRequestHandler<GetProductVariantsQuery, Result<IReadOnlyList<ProductVariantResponse>>>
    {
        private readonly IProductVariantQueries _variantQueries;

        public GetProductVariantsQueryHandler(IProductVariantQueries variantQueries)
        {
            _variantQueries = variantQueries;
        }

        public async Task<Result<IReadOnlyList<ProductVariantResponse>>> Handle(
            GetProductVariantsQuery request, CancellationToken cancellationToken)
        {
            var variants = await _variantQueries.GetByProductIdAsync(request.ProductId, cancellationToken);
            return Result<IReadOnlyList<ProductVariantResponse>>.Success(variants);
        }
    }
}
