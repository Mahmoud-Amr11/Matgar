using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Application.Features.Orders.Commands.CancelOrder;
using Matgar.Application.Features.Orders.Commands.Checkout;
using Matgar.Application.Features.Orders.Queries.GetOrderById;
using Matgar.Application.Features.Orders.Queries.GetOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost("checkout")]
        public async Task<IActionResult> Checkout(CheckoutCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet]
        public async Task<IActionResult> GetUserOrders(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetOrdersQuery(), cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetOrderByIdQuery(id), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("{id}/cancel")]
        public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CancelOrderCommand(id), cancellationToken);
            return result.ToActionResult();
        }
    }
}
