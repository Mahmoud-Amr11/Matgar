using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Payments.Queries.GetPaymentStatus;
using MediatR;

namespace Matgar.Application.Features.Payments.Handlers
{
    public class GetPaymentStatusQueryHandler : IRequestHandler<GetPaymentStatusQuery, Result<PaymentStatusResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public GetPaymentStatusQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<PaymentStatusResponse>> Handle(GetPaymentStatusQuery request, CancellationToken cancellationToken)
        {
            try
            {
                var payments = await _unitOfWork.Payments.FindAsync(p => p.OrderId == request.OrderId, cancellationToken: cancellationToken);
                var pay = payments?.FirstOrDefault();
                if (pay == null) return Error.NotFound(message: "Payment not found for order.");

                return Result<PaymentStatusResponse>.Success(new PaymentStatusResponse(pay.TransactionReference ?? string.Empty, pay.Status.ToString()));
            }
            catch
            {
                return Error.Failure(message: "Failed to get payment status.");
            }
        }
    }
}
