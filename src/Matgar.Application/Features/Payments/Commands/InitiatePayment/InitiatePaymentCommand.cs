using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Payments.Commands.InitiatePayment
{
    public sealed record InitiatePaymentCommand(Guid OrderId, decimal Amount, string Currency = "USD") : IRequest<Result<InitiatePaymentResponse>>;

    public sealed record InitiatePaymentResponse(string ClientSecret, string TransactionReference);
}
