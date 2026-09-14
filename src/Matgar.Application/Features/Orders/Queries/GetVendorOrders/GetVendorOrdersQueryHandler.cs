using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetVendorOrders
{
    public class GetVendorOrdersQueryHandler : IRequestHandler<GetVendorOrdersQuery, Result<IReadOnlyList<VendorOrderResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICurrentUserService _currentUser;

        public GetVendorOrdersQueryHandler(IUnitOfWork unitOfWork, ICurrentUserService currentUser)
        {
            _unitOfWork = unitOfWork;
            _currentUser = currentUser;
        }

        public async Task<Result<IReadOnlyList<VendorOrderResponse>>> Handle(GetVendorOrdersQuery request, CancellationToken cancellationToken)
        {
            if (!Guid.TryParse(_currentUser.UserId, out var vendorId))
                return Error.Unauthorized(message: "Invalid vendor identity.");

            // find orders that contain items for products owned by this vendor
            var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);

            var vendorOrders = orders.Where(o => o.Items.Any(i => i.ProductVariant.Product.VendorId == vendorId)).Select(o => new VendorOrderResponse
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            }).ToList();

            return Result<IReadOnlyList<VendorOrderResponse>>.Success(vendorOrders);
        }
    }
}
