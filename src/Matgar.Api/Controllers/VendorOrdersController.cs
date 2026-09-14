using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Application.Features.Orders.Commands.UpdateVendorOrderStatus;
using Matgar.Application.Features.Orders.Queries.GetVendorOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/vendor/[controller]")]
    [ApiController]
    [Authorize(Roles = "Vendor")]
    public class VendorOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public VendorOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetVendorOrders(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetVendorOrdersQuery(), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(Guid id, UpdateVendorOrderStatusCommand request, CancellationToken cancellationToken)
        {
            // ensure route id matches body id
            if (id != request.OrderId) return BadRequest();

            var result = await _mediator.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
