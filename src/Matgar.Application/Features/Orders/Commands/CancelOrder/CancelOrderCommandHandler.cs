using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Domain.Entities;
using MediatR;

namespace Matgar.Application.Features.Orders.Commands.CancelOrder
{
    public class CancelOrderCommandHandler : IRequestHandler<CancelOrderCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CancelOrderCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(CancelOrderCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var order = (await _unitOfWork.Orders.FindAsync(o => o.Id == request.OrderId && o.CustomerId == userId, cancellationToken)).FirstOrDefault();
            if (order == null) return Error.NotFound(code: "Order.NotFound", message: "Order not found.");

            if (order.Status >= OrderStatus.Shipped)
                return Error.Validation(code: "Order.CannotCancel", message: "Order cannot be cancelled after it is shipped.");

            await _unitOfWork.BeginTransactionAsync(cancellationToken);
            try
            {
                // revert reserved quantities
                foreach (var item in order.Items)
                {
                    var stock = (await _unitOfWork.StockItems.FindAsync(s => s.ProductVariantId == item.ProductVariantId, cancellationToken)).FirstOrDefault();
                    if (stock != null)
                    {
                        stock.QuantityReserved = Math.Max(0, stock.QuantityReserved - item.Quantity);
                        _unitOfWork.StockItems.Update(stock);
                    }
                }

                order.Status = OrderStatus.Cancelled;
                order.CancelledAt = DateTime.UtcNow;
                _unitOfWork.Orders.Update(order);

                var outbox = new OutboxMessage
                {
                    Type = "OrderCancelled",
                    Content = System.Text.Json.JsonSerializer.Serialize(new { OrderId = order.Id }),
                    OccurredOn = DateTime.UtcNow
                };

                await _unitOfWork.OutboxMessages.AddAsync(outbox, cancellationToken);

                await _unitOfWork.CommitTransactionAsync(cancellationToken);

                return Result.Success;
            }
            catch (Exception ex)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Error.Failure(message: ex.Message);
            }
        }
    }
}
