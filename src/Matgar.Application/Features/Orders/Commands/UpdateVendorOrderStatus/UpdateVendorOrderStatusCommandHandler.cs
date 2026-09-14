using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Domain.Entities;
using MediatR;

namespace Matgar.Application.Features.Orders.Commands.UpdateVendorOrderStatus
{
    public class UpdateVendorOrderStatusCommandHandler : IRequestHandler<UpdateVendorOrderStatusCommand, Result>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public UpdateVendorOrderStatusCommandHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result> Handle(UpdateVendorOrderStatusCommand request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            var order = (await _unitOfWork.Orders.FindAsync(o => o.Id == request.OrderId, cancellationToken)).FirstOrDefault();
            if (order == null) return Error.NotFound(code: "Order.NotFound", message: "Order not found.");

            // ensure this vendor has items in this order
            if (!order.Items.Any(i => i.ProductVariant.Product.VendorId == vendorId))
                return Error.Forbidden(code: "Order.Forbidden", message: "You do not have permission to modify this order.");

            var newStatus = (OrderStatus)request.NewStatus;

            // validate transitions: Pending -> Confirmed -> Shipped -> Delivered
            var valid = newStatus switch
            {
                OrderStatus.Confirmed => order.Status == OrderStatus.Pending,
                OrderStatus.Shipped => order.Status == OrderStatus.Confirmed,
                OrderStatus.Delivered => order.Status == OrderStatus.Shipped,
                _ => false
            };

            if (!valid) return Error.Validation(code: "Order.InvalidTransition", message: "Invalid status transition.");

            order.Status = newStatus;
            if (newStatus == OrderStatus.Confirmed) order.ConfirmedAt = DateTime.UtcNow;
            if (newStatus == OrderStatus.Shipped) order.ShippedAt = DateTime.UtcNow;

            _unitOfWork.Orders.Update(order);

            var outbox = new OutboxMessage
            {
                Type = "OrderStatusUpdated",
                Content = System.Text.Json.JsonSerializer.Serialize(new { OrderId = order.Id, Status = order.Status.ToString() }),
                OccurredOn = DateTime.UtcNow
            };

            await _unitOfWork.OutboxMessages.AddAsync(outbox, cancellationToken);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
    }
}
