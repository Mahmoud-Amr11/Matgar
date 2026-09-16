using Matgar.Api.Common;
using Matgar.Application.Features.Payments.Commands.HandleWebhook;
using Matgar.Application.Features.Payments.Commands.InitiatePayment;
using Matgar.Application.Features.Payments.Queries.GetPaymentStatus;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PaymentsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public PaymentsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("initiate")]
        [Authorize]
        public async Task<IActionResult> Initiate(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.ToActionResult();
        }

        // Webhook endpoint - called by payment gateway, should be unauthenticated in practice
        [HttpPost("webhook")]
        [AllowAnonymous]
        public async Task<IActionResult> Webhook(HandlePaymentWebhookCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("{orderId}/status")]
        [Authorize]
        public async Task<IActionResult> GetStatus(Guid orderId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetPaymentStatusQuery(orderId), cancellationToken);
            return result.ToActionResult();
        }
    }
}
