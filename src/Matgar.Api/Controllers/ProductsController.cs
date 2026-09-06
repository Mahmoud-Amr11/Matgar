using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Api.Requests.Products;
using Matgar.Application.Features.Products.Commands.ApproveProduct;
using Matgar.Application.Features.Products.Commands.CreateProduct;
using Matgar.Application.Features.Products.Commands.DeleteProduct;
using Matgar.Application.Features.Products.Commands.SubmitProductForReview;
using Matgar.Application.Features.Products.Commands.UpdateProduct;
using Matgar.Application.Features.Products.Queries.GetAllProducts;
using Matgar.Application.Features.Products.Queries.GetProductById;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class ProductsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            [FromQuery] string? search,
            [FromQuery] Guid? categoryId,
            [FromQuery] decimal? minPrice,
            [FromQuery] decimal? maxPrice,
            [FromQuery] ProductStatus? status,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetProductsQuery(search, categoryId, minPrice, maxPrice, status, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetProductByIdQuery(id), cancellationToken);
            return result.ToActionResult();
        }




        [HttpPost]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Create(
    [FromBody] CreateProductCommand command,
    CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetById), new { id = result.Value }, new { id = result.Value })
                : result.ToActionResult();
        }

        [HttpPut("{id:guid}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateProductRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateProductCommand(id, request.Name, request.Description, request.CategoryId);
            var result = await _mediator.Send(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{id:guid}")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteProductCommand(id), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("{id:guid}/submit-for-review")]
        [Authorize(Roles = "Vendor")]
        public async Task<IActionResult> SubmitForReview(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SubmitProductForReviewCommand(id), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("{id:guid}/approve")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Approve(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ApproveProductCommand(id), cancellationToken);
            return result.ToActionResult();
        }
    }
}
