using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Application.Features.Coupons.Commands.CreateCoupon;
using Matgar.Application.Features.Coupons.Queries.GetAllCoupons;
using Matgar.Application.Features.Coupons.Queries.ValidateCoupon;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class CouponsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CouponsController(IMediator mediator)
        {
            _mediator = mediator;
        }


        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create(CreateDiscountCommand request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.IsSuccess
              ? Created(
                  string.Empty,
                  new { couponId = result.Value })
              : result.ToActionResult();
        }

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAllCouponsQuery(), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost("validate")]
        public async Task<IActionResult> Validate(ValidateCouponQuery request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(request, cancellationToken);
            return result.ToActionResult();
        }
    }
}
