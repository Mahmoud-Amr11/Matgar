using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Abstractions.Services;
using MediatR;
using Microsoft.Extensions.Logging;

namespace Matgar.Application.Features.Payments.Commands.HandlePaymobWebhook
{
    public sealed class HandlePaymobWebhookCommandHandler
        : IRequestHandler<HandlePaymobWebhookCommand, WebhookOutcome>
    {
        private readonly IPaymentGateway _gateway;
        private readonly IUnitOfWork _uow;
        private readonly ILogger<HandlePaymobWebhookCommandHandler> _logger;

        public HandlePaymobWebhookCommandHandler(
            IPaymentGateway gateway, IUnitOfWork uow,
            ILogger<HandlePaymobWebhookCommandHandler> logger)
        {
            _gateway = gateway;
            _uow = uow;
            _logger = logger;
        }

        public async Task<WebhookOutcome> Handle(HandlePaymobWebhookCommand cmd, CancellationToken ct)
        {
            // 1) Verify signature
            var evt = await _gateway.VerifyWebhookAsync(cmd.RawBody, cmd.Hmac, ct);
            if (evt is null)
            {
                _logger.LogWarning("Paymob webhook rejected: invalid signature");
                return WebhookOutcome.InvalidSignature;
            }

            // 2) Idempotency: the same transaction + state must be processed once
            var eventKey = $"paymob:{evt.TransactionId}:{evt.Success}:{evt.Pending}:{evt.IsRefunded}:{evt.IsVoided}";
            if (!await _uow.ProcessedWebhooks.TryRegisterAsync(eventKey, ct))   // INSERT with UNIQUE index; false on duplicate
                return WebhookOutcome.Duplicate;

            // 3) Load the order by our reference
            var order = await _uow.Orders.GetByReferenceAsync(evt.OrderReference, ct);
            if (order is null)
            {
                _logger.LogWarning("Paymob webhook for unknown order reference {Reference}", evt.OrderReference);
                return WebhookOutcome.Ignored;
            }

            // 4) Never trust the webhook alone: validate the amount
            var expectedCents = (long)Math.Round(order.Total * 100, MidpointRounding.AwayFromZero);
            if (evt.Success && evt.AmountCents != expectedCents)
            {
                _logger.LogError("Amount mismatch on order {Ref}: expected {Expected}, got {Actual}",
                    evt.OrderReference, expectedCents, evt.AmountCents);
                return WebhookOutcome.Ignored;
            }

            // 5) Apply the state transition (the domain must reject invalid transitions)
            if (evt.IsRefunded) order.MarkRefunded();
            else if (evt.IsVoided) order.MarkVoided();
            else if (evt.Success) order.MarkPaid(evt.TransactionId.ToString());
            else if (!evt.Pending) order.MarkPaymentFailed();

            await _uow.SaveChangesAsync(ct);
            return WebhookOutcome.Processed;
        }
    }
}