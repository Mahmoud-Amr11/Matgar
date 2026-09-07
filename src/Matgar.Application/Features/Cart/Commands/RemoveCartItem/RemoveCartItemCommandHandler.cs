using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.RemoveCartItem
{
    public class RemoveCartItemCommandHandler : IRequestHandler<RemoveCartItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public RemoveCartItemCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(RemoveCartItemCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId, cancellationToken);
            if (cartItem is null)
                return Error.NotFound(code: "CartItem.NotFound", message: "Cart item not found.");

            var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId, cancellationToken);
            if (cart is null || cart.UserId != userId)
                return Error.NotFound(code: "CartItem.NotFound", message: "Cart item not found.");

            _unitOfWork.CartItems.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
