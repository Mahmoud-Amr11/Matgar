using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Application.Features.Orders.Queries.GetAllOrders;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/admin/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminOrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AdminOrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllOrdersQuery(), cancellationToken);
            return result.ToActionResult();
        }
    }
}
