using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Cart.Queries.Responses;
using MediatR;

namespace Matgar.Application.Features.Cart.Commands.AddCartItem
{
    public class AddCartItemCommandHandler : IRequestHandler<AddCartItemCommand, Result<AddCartItemResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public AddCartItemCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<AddCartItemResponse>> Handle(AddCartItemCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var variant = await _unitOfWork.ProductVariants.GetByIdAsync(request.ProductVariantId, cancellationToken);
            if (variant is null)
                return Error.NotFound(code: "ProductVariant.NotFound", message: "Product variant not found.");

            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken);
            var isNewCart = cart is null;

            if (isNewCart)
            {
                cart = new Domain.Entities.Cart
                {
                    Id = Guid.NewGuid(),
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };
            }

            var existingItem = await _unitOfWork.CartItems.GetByCartAndVariantAsync(
                cart!.Id, request.ProductVariantId, cancellationToken);

            var resultingQuantity = request.Quantity + (existingItem?.Quantity ?? 0);

            var stockItems = await _unitOfWork.StockItems.FindAsync(
                s => s.ProductVariantId == request.ProductVariantId, cancellationToken);

            var availableQuantity = stockItems.FirstOrDefault()?.AvailableQuantity ?? 0;

            if (resultingQuantity > availableQuantity)
            {
                return Error.Conflict(
                    code: "ProductVariant.InsufficientStock",
                    message: $"Only {availableQuantity} unit(s) available for this variant.");
            }

            if (isNewCart)
            {
                await _unitOfWork.Carts.AddAsync(cart!, cancellationToken);
            }

            if (existingItem is not null)
            {
                existingItem.Quantity = resultingQuantity;
                _unitOfWork.CartItems.Update(existingItem);
                await _unitOfWork.SaveChangesAsync(cancellationToken);

                return new AddCartItemResponse(
                    CartId: cart!.Id,
                    CartItemId: existingItem.Id,
                    Quantity: existingItem.Quantity);
            }

            var cartItem = new Domain.Entities.CartItem
            {
                Id = Guid.NewGuid(),
                CartId = cart!.Id,
                ProductVariantId = request.ProductVariantId,
                Quantity = request.Quantity,
                PriceSnapshot = variant.Price
            };

            await _unitOfWork.CartItems.AddAsync(cartItem, cancellationToken);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new AddCartItemResponse(
                CartId: cart.Id,
                CartItemId: cartItem.Id,
                Quantity: cartItem.Quantity);
        }
    }
}