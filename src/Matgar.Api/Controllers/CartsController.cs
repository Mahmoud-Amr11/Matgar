using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Api.Requests.Cart;
using Matgar.Application.Features.Cart.Commands.AddCartItem;
using Matgar.Application.Features.Cart.Commands.ClearCart;
using Matgar.Application.Features.Cart.Commands.RemoveCartItem;
using Matgar.Application.Features.Cart.Commands.UpdateCartItem;
using Matgar.Application.Features.Cart.Queries.GetCart;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize(Roles = "Customer")]
    public class CartsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CartsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetCart(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetCartQuery(), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("items")]
        public async Task<IActionResult> AddItem(
            [FromBody] AddCartItemRequest request,
            CancellationToken cancellationToken)
        {
            var command = new AddCartItemCommand(request.ProductVariantId, request.Quantity);
            var result = await _mediator.Send(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("items/{itemId:guid}")]
        public async Task<IActionResult> UpdateItem(
            Guid itemId,
            [FromBody] UpdateCartItemRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateCartItemCommand(itemId, request.Quantity);
            var result = await _mediator.Send(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("items/{itemId:guid}")]
        public async Task<IActionResult> RemoveItem(Guid itemId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new RemoveCartItemCommand(itemId), cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete]
        public async Task<IActionResult> ClearCart(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ClearCartCommand(), cancellationToken);
            return result.ToActionResult();
        }
    }
}
