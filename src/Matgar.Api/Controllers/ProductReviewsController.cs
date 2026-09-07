using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Api.Requests.ProductReview;
using Matgar.Application.Features.ProductReview.Commands.CreateProductReview;
using Matgar.Application.Features.ProductReview.Queries.GetProductReviews;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/products/{productId:guid}/reviews")]
    [ApiController]
    public class ProductReviewsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ProductReviewsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(
            Guid productId,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20,
            CancellationToken cancellationToken = default)
        {
            var query = new GetProductReviewsQuery(productId, page, pageSize);
            var result = await _mediator.Send(query, cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost]
        [Authorize(Roles = "Customer")]
        public async Task<IActionResult> Create(
            Guid productId,
            [FromBody] CreateProductReviewRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateProductReviewCommand(productId, request.Rating, request.Comment);
            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetAll), new { productId }, new { reviewId = result.Value })
                : result.ToActionResult();
        }
    }
}
