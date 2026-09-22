using MediatR;

namespace Matgar.Application.Features.Payments.Commands.HandlePaymobWebhook
{
    public enum WebhookOutcome { Processed, Duplicate, InvalidSignature, Ignored }

    public sealed record HandlePaymobWebhookCommand(string RawBody, string Hmac) : IRequest<WebhookOutcome>;
}
