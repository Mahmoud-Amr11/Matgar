using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Api.Requests.Addresses;
using Matgar.Application.Features.Addresses.Commands.CreateAddress;
using Matgar.Application.Features.Addresses.Commands.DeleteAddress;
using Matgar.Application.Features.Addresses.Commands.SetDefaultAddress;
using Matgar.Application.Features.Addresses.Commands.UpdateAddress;
using Matgar.Application.Features.Addresses.Queries.GetAddresses;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]
    [Authorize]
    public class AddressesController : ControllerBase
    {
        private readonly IMediator _mediator;

        public AddressesController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new GetAddressesQuery(), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPost]
        public async Task<IActionResult> Create(
            [FromBody] CreateAddressRequest request,
            CancellationToken cancellationToken)
        {
            var command = new CreateAddressCommand(
                request.FullAddress,
                request.City,
                request.Governorate,
                request.PhoneNumber,
                request.IsDefault);

            var result = await _mediator.Send(command, cancellationToken);

            return result.IsSuccess
                ? CreatedAtAction(nameof(GetAll), new { }, new { addressId = result.Value })
                : result.ToActionResult();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(
            Guid id,
            [FromBody] UpdateAddressRequest request,
            CancellationToken cancellationToken)
        {
            var command = new UpdateAddressCommand(
                id,
                request.FullAddress,
                request.City,
                request.Governorate,
                request.PhoneNumber);

            var result = await _mediator.Send(command, cancellationToken);
            return result.ToActionResult();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new DeleteAddressCommand(id), cancellationToken);
            return result.ToActionResult();
        }

        [HttpPut("{id:guid}/set-default")]
        public async Task<IActionResult> SetDefault(Guid id, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new SetDefaultAddressCommand(id), cancellationToken);
            return result.ToActionResult();
        }
    }
}