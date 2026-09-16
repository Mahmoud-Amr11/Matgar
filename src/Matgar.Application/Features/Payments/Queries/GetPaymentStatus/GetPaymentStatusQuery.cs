using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Payments.Queries.GetPaymentStatus
{
    public sealed record GetPaymentStatusQuery(Guid OrderId) : IRequest<Result<PaymentStatusResponse>>;

    public sealed record PaymentStatusResponse(string TransactionReference, string Status);
}
