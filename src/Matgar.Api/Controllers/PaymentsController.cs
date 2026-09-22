using Matgar.Api.Common;
using Matgar.Application.Features.Payments.Commands.CreatePayment;
using Matgar.Application.Features.Payments.Commands.HandlePaymobWebhook;
using Matgar.Application.Features.Payments.PaymentDtos;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers;

[ApiController]
[Route("api/payments")]
public sealed class PaymentsController : ControllerBase
{
    private readonly ISender _sender;
    private readonly ILogger<PaymentsController> _logger;

    public PaymentsController(ISender sender, ILogger<PaymentsController> logger)
    {
        _sender = sender;
        _logger = logger;
    }

    /// <summary>Creates a Paymob payment intention for an order.</summary>
    [HttpPost("orders/{orderId:guid}")]
    [Authorize]
    [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
    public async Task<IActionResult> CreatePayment(
        Guid orderId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
            return Problem(
                title: "Missing Idempotency-Key header",
                statusCode: StatusCodes.Status400BadRequest);

        var result = await _sender.Send(new CreatePaymentCommand(orderId, idempotencyKey), ct);
        return result.ToActionResult();
    }

    /// <summary>Paymob transaction callback (server-to-server). Never called by our client.</summary>
    [HttpPost("webhook")]
    [AllowAnonymous]
    public async Task<IActionResult> Webhook(
        [FromQuery] string? hmac,
        CancellationToken ct)
    {
        // Read the raw body: the HMAC is computed over parsed values, but we still want the raw payload for audit
        using var reader = new StreamReader(Request.Body);
        var rawBody = await reader.ReadToEndAsync(ct);

        if (string.IsNullOrWhiteSpace(hmac))
        {
            _logger.LogWarning("Paymob webhook rejected: missing hmac");
            return Unauthorized();
        }

        var handled = await _sender.Send(new HandlePaymobWebhookCommand(rawBody, hmac), ct);

        // Return 200 for both "processed" and "already processed" so Paymob doesn't retry.
        // Only invalid signatures are rejected (401).
        return handled == WebhookOutcome.InvalidSignature ? Unauthorized() : Ok();
    }
}