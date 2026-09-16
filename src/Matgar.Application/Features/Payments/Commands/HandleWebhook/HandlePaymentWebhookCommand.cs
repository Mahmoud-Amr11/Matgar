using Medgar = MediatR;
using Matgar.Application.Common.Results;

namespace Matgar.Application.Features.Payments.Commands.HandleWebhook
{
    public sealed record HandlePaymentWebhookCommand(string TransactionReference, string Status, string GatewayPayload) : Medgar.IRequest<Result<bool>>;
}
