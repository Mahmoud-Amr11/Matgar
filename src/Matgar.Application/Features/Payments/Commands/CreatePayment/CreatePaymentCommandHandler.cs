using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Abstractions.Services;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs.Payments;
using MediatR;

namespace Matgar.Application.Features.Payments.Commands.CreatePayment
{
    public sealed class CreatePaymentCommandHandler : IRequestHandler<CreatePaymentCommand, Result<PaymentResult>>
    {
        private readonly IPaymentGateway _gateway;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public CreatePaymentCommandHandler(IPaymentGateway gateway, IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _gateway = gateway;
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<PaymentResult>> Handle(CreatePaymentCommand cmd, CancellationToken ct)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var order = await _unitOfWork.Orders.GetByIdAsync(cmd.OrderId, ct);
            if (order is null)
                return Error.NotFound(code: "Order.NotFound", message: "Order not found.");

            if (order.CustomerId != userId)
                return Error.Forbidden(code: "Order.Forbidden", message: "You cannot pay for this order.");

            if (order.IsPaid)
                return Error.Conflict(code: "Payment.AlreadyPaid", message: "Order already paid.");

            var customerName = (_currentUser.UserName?.Trim()
                ?? _currentUser.UserEmail?.Split('@')[0]
                ?? "Guest")
                .Trim();

            var firstName = customerName.Split(' ', 2)[0];
            var lastName = firstName.Length < customerName.Length
                ? customerName[(firstName.Length + 1)..]
                : "Customer";

            var request = new PaymentRequest
            {
                Amount = order.Total,
                Currency = "EGP",
                OrderReference = order.Reference,
                CustomerFirstName = firstName,
                CustomerLastName = lastName,
                CustomerEmail = _currentUser.UserEmail ?? string.Empty,
                CustomerPhone = string.Empty,
                Items = order.Items.Select(i => new PaymentItem
                {
                    Name = i.ProductVariant?.Sku ?? i.ProductVariantId.ToString(),
                    Amount = i.UnitPrice,
                    Description = i.ProductVariant?.Sku ?? i.ProductVariantId.ToString(),
                    Quantity = i.Quantity
                }).ToList()
            };

            var result = await _gateway.CreatePaymentAsync(request, ct);
            return Result<PaymentResult>.Success(result);
        }
    }
}