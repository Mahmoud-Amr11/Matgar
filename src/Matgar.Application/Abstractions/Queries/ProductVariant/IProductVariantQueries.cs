using Matgar.Application.Features.Products.Queries.Responses;

namespace Matgar.Application.Abstractions.Queries.ProductVariant
{
    public interface IProductVariantQueries
    {
        Task<IReadOnlyList<ProductVariantResponse>> GetByProductIdAsync(
            Guid productId, CancellationToken cancellationToken);
    }
}
