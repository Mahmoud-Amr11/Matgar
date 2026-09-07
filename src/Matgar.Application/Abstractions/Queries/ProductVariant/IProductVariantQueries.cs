using Matgar.Application.Features.ProductVariant.Queries.Responses;

namespace Matgar.Application.Abstractions.Queries.ProductVariant
{
    public interface IProductVariantQueries
    {
        Task<IReadOnlyList<ProductVariantResponse>> GetByProductIdAsync(
            Guid productId, CancellationToken cancellationToken);
    }
}