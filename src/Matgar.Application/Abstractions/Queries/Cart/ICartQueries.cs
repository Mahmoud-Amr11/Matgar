using Matgar.Application.Features.Cart.Queries.Responses;

namespace Matgar.Application.Abstractions.Queries.Cart
{
    public interface ICartQueries
    {
        Task<CartResponse?> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken);
    }
}