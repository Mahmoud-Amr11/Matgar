using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetOrderById
{
    public class GetOrderByIdQueryHandler : IRequestHandler<GetOrderByIdQuery, Result<OrderDetailResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetOrderByIdQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<OrderDetailResponse>> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var order = (await _unitOfWork.Orders.FindAsync(o => o.Id == request.OrderId && o.CustomerId == userId, cancellationToken)).FirstOrDefault();

            if (order == null) return Error.NotFound(code: "Order.NotFound", message: "Order not found.");

            var response = new OrderDetailResponse
            {
                Id = order.Id,
                CreatedAt = order.CreatedAt,
                SubTotal = order.SubTotal,
                DiscountAmount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                Status = order.Status.ToString(),
                ShippingAddressSnapshot = order.ShippingAddressSnapshot,
                Items = order.Items.Select(i => new OrderItemResponse
                {
                    ProductVariantId = i.ProductVariantId,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice,
                    Sku = i.ProductVariant?.Sku ?? string.Empty
                }).ToList()
            };

            return Result<OrderDetailResponse>.Success(response);
        }
    }
}
