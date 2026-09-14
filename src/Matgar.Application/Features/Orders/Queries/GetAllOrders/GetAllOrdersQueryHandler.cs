using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Orders.Queries.GetAllOrders
{
    public class GetAllOrdersQueryHandler : IRequestHandler<GetAllOrdersQuery, Result<IReadOnlyList<GetAllOrdersResponse>>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetAllOrdersQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<IReadOnlyList<GetAllOrdersResponse>>> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
        {
            var orders = await _unitOfWork.Orders.GetAllAsync(cancellationToken);

            var response = orders.Select(o => new GetAllOrdersResponse
            {
                Id = o.Id,
                CreatedAt = o.CreatedAt,
                TotalAmount = o.TotalAmount,
                Status = o.Status.ToString()
            }).ToList();

            return Result<IReadOnlyList<GetAllOrdersResponse>>.Success(response);
        }
    }
}
