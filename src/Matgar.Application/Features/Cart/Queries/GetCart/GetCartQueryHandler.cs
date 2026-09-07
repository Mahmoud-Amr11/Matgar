using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Queries.Cart;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Queries.GetCart
{
    public class GetCartQueryHandler : IRequestHandler<GetCartQuery, Result<Responses.CartResponse>>
    {
        private readonly ICartQueries _cartQueries;
        private readonly ICurrentUserService _currentUser;

        public GetCartQueryHandler(ICartQueries cartQueries, ICurrentUserService currentUser)
        {
            _cartQueries = cartQueries;
            _currentUser = currentUser;
        }

        public async Task<Result<Responses.CartResponse>> Handle(GetCartQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var cart = await _cartQueries.GetByUserIdAsync(userId, cancellationToken);

            if (cart is null)
            {
                return new Responses.CartResponse(
                    CartId: Guid.Empty,
                    UserId: userId,
                    CreatedAt: DateTime.UtcNow,
                    Items: [],
                    TotalPrice: 0m);
            }

            return cart;
        }
    }
}