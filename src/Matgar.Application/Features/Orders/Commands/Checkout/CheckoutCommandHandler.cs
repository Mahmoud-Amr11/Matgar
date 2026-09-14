using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Domain.Entities;
using MediatR;
using System.Text.Json;

namespace Matgar.Application.Features.Orders.Commands.Checkout
{
    public class CheckoutCommandHandler : IRequestHandler<CheckoutCommand, Result<Guid>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CheckoutCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<Guid>> Handle(CheckoutCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var cart = await _unitOfWork.Carts.GetByUserIdAsync(userId, cancellationToken);
            if (cart is null || cart.Items == null || !cart.Items.Any())
                return Error.Validation(code: "Cart.Empty", message: "Cart is empty.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                decimal subTotal = 0m;

                // check stock and reserve
                foreach (var item in cart.Items)
                {
                    var variant = item.ProductVariant;
                    var stock = (await _unitOfWork.StockItems.FindAsync(s => s.ProductVariantId == variant.Id, cancellationToken)).FirstOrDefault();

                    if (stock == null) return Error.NotFound(code: "Stock.NotFound", message: "Stock item not found.");

                    if (stock.AvailableQuantity < item.Quantity)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Error.Validation(code: "Stock.Insufficient", message: $"Insufficient stock for variant {variant.Sku}.");
                    }

                    stock.QuantityReserved += item.Quantity;
                    _unitOfWork.StockItems.Update(stock);

                    subTotal += item.Quantity * item.PriceSnapshot;
                }

                // coupon
                Guid? couponId = null;
                decimal discountAmount = 0m;

                if (!string.IsNullOrWhiteSpace(request.CouponCode))
                {
                    var code = request.CouponCode.Trim();
                    var coupon = (await _unitOfWork.Coupons.FindAsync(c => c.Code.ToLower() == code.ToLower(), cancellationToken)).FirstOrDefault();
                    if (coupon == null)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Error.NotFound(code: "Coupon.NotFound", message: "Coupon not found.");
                    }

                    if (!coupon.IsValid)
                    {
                        await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                        return Error.Validation(code: "Coupon.Invalid", message: "Coupon is not valid.");
                    }

                    couponId = coupon.Id;

                    discountAmount = coupon.DiscountType == DiscountType.Percentage
                        ? Math.Round(subTotal * coupon.DiscountValue / 100m, 2)
                        : coupon.DiscountValue;

                    if (discountAmount > subTotal) discountAmount = subTotal;
                }

                var order = new Order
                {
                    CustomerId = userId,
                    SubTotal = subTotal,
                    DiscountAmount = discountAmount,
                    TotalAmount = subTotal - discountAmount,
                    CouponId = couponId,
                    ShippingAddressSnapshot = (await _unitOfWork.Addresses.GetByIdAsync(request.AddressId, cancellationToken))?.ToString() ?? string.Empty,
                };

                foreach (var item in cart.Items)
                {
                    order.Items.Add(new OrderItem
                    {
                        ProductVariantId = item.ProductVariantId,
                        Quantity = item.Quantity,
                        UnitPrice = item.PriceSnapshot
                    });
                }

                await _unitOfWork.Orders.AddAsync(order, cancellationToken);

                var outbox = new OutboxMessage
                {
                    Type = "OrderCreated",
                    Content = JsonSerializer.Serialize(new { OrderId = order.Id, CustomerId = order.CustomerId }),
                    OccurredOn = DateTime.UtcNow
                };

                await _unitOfWork.OutboxMessages.AddAsync(outbox, cancellationToken);

                // remove cart
                _unitOfWork.Carts.Remove(cart);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result<Guid>.Success(order.Id);
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Error.Failure(message: ex.Message);
            }
        }
    }
}
