using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.UpdateCartItem
{
    public class UpdateCartItemCommandHandler : IRequestHandler<UpdateCartItemCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateCartItemCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var cartItem = await _unitOfWork.CartItems.GetByIdAsync(request.CartItemId, cancellationToken);
            if (cartItem is null)
                return Error.NotFound(code: "CartItem.NotFound", message: "Cart item not found.");

            var cart = await _unitOfWork.Carts.GetByIdAsync(cartItem.CartId, cancellationToken);
            if (cart is null || cart.UserId != userId)
                return Error.NotFound(code: "CartItem.NotFound", message: "Cart item not found.");

            var stockItems = await _unitOfWork.StockItems.FindAsync(
                s => s.ProductVariantId == cartItem.ProductVariantId, cancellationToken);

            var availableQuantity = stockItems.FirstOrDefault()?.AvailableQuantity ?? 0;

            if (request.Quantity > availableQuantity)
            {
                return Error.Conflict(
                    code: "ProductVariant.InsufficientStock",
                    message: $"Only {availableQuantity} unit(s) available for this variant.");
            }

            cartItem.Quantity = request.Quantity;
            _unitOfWork.CartItems.Update(cartItem);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}