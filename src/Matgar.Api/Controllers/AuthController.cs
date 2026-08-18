using Matgar.Api.Common;
using Matgar.Application.Features.Auth.Commands.ConfirmEmail;
using Matgar.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;


        public AuthController(IMediator mediator)
        {
            _mediator = mediator;
        }
        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterCommand registerCommand, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(registerCommand, cancellationToken);
            return result.ToActionResult(this);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConfirmEmailCommand(userId, token), cancellationToken);
            return result.ToActionResult(this);
        }
    }
}