using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetOrders
{
    public class GetOrdersQueryHandler : IRequestHandler<GetOrdersQuery, Result<IReadOnlyList<OrderResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<OrderResponse>>> Handle(GetOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var userId))
                return Error.Unauthorized(message: "Invalid user identity.");

            var orders = await _unitOfWork.Orders.FindAsync(o => o.CustomerId == userId, cancellationToken);

            var response = orders.Select(o => new OrderResponse
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                SubTotal = o.SubTotal,
                DiscountAmount = o.DiscountAmount,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            }).ToList();

            return Result<IReadOnlyList<OrderResponse>>.Success(response);
        }
    }
}
