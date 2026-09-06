using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Api.Requests.ProductVariants;
using Matgar.Application.Features.ProductVariant.Commands.CreateProductVariant;
using Matgar.Application.Features.ProductVariant.Commands.DeleteProductVariant;
using Matgar.Application.Features.ProductVariant.Commands.UpdateProductVariant;
using Matgar.Application.Features.ProductVariant.Queries.GetProductVariants;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/products/{productId:guid}/variants")]
    [ApiController]
    public class ProductVariantsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductVariantsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(Guid productId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductVariantsQuery(productId), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Create(
            Guid productId,
            [FromBody] CreateProductVariantRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateProductVariantCommand(
                productId, request.Sku, request.Price, request.ImageUrl,
                request.AttributesJson, request.InitialQuantity);

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetAll), new { productId }, new { variantId = result.Value })
                : result.ToActionResult();
        }

        [HttpPut("{variantId:guid}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Update(
            Guid productId,
            Guid variantId,
            [FromBody] UpdateProductVariantRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductVariantCommand(
                productId, variantId, request.Price, request.ImageUrl, request.AttributesJson);

            var result = await _mediator.Send(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{variantId:guid}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Delete(Guid productId, Guid variantId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProductVariantCommand(productId, variantId), cancellationToken);
            return result.ToActionResult();
        }
    }
}
