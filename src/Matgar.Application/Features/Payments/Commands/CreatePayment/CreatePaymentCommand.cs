using Matgar.Application.Common.Results;
using Matgar.Application.Features.Payments.PaymentDtos;
using MediatR;

namespace Matgar.Application.Features.Payments.Commands.CreatePayment
{
    public sealed record CreatePaymentCommand(Guid OrderId, string IdempotencyKey) : IRequest<Result<PaymentResult>>;
}