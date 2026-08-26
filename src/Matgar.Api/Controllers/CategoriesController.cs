using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Api.Requests.Categories;
using Matgar.Application.Features.Category.Commands.CreateCategory;
using Matgar.Application.Features.Category.Commands.DeleteCategory;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoriesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new CreateCategoryCommand(request.CategoryName), cancellationToken);

            return result.ToActionResult();
        }
        [HttpDelete("{CategoryId:Guid}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(Guid CategoryId, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteCategoryCommand(CategoryId), cancellationToken);

            return result.ToActionResult();
        }
    }
}
