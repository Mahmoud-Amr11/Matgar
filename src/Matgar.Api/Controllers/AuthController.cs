using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Auth.Commands.ConfirmEmail;
using Matgar.Application.Features.Auth.Commands.Login;
using Matgar.Application.Features.Auth.Commands.RefreshToken;
using Matgar.Application.Features.Auth.Commands.Register;
using Matgar.Application.Features.Auth.Commands.RevokeToken;
using Matgar.Application.Features.Auth.Responses;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Matgar.Api.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
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
            return result.ToActionResult();
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginCommand loginCommand, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(loginCommand, cancellationToken);
            if (result.IsSuccess)
            {
                SetRefreshTokenInCookies(
                    result.Value.RefreshToken,
                    result.Value.RefreshTokenExpiresOn);
                var response = new LoginResponse(
                                             result.Value.UserId,
                                             result.Value.Email,
                                             result.Value.AccessToken,
                                             result.Value.AccessTokenExpiresAt);

                return Ok(response);
            }

            return result.ToActionResult();

        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConfirmEmailCommand(userId, token), cancellationToken);
            return result.ToActionResult();
        }


        [HttpGet("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["__Host-refreshToken"];

            if (refreshToken is null)
                return Result.Failure(Error.Unauthorized(message: "Invalid Token")).ToActionResult();

            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));

            if (!result.IsSuccess)
            {
                return result.ToActionResult();
            }

            if (result.IsSuccess)
            {
                SetRefreshTokenInCookies(
                    result.Value.RefreshToken,
                    result.Value.RefreshTokenExpiresOn);
            }

            var response = new LoginResponse(
                       result.Value.UserId,
                       result.Value.Email,
                       result.Value.AccessToken,
                       result.Value.AccessTokenExpiresAt
                   );


            return Ok(response);
        }

        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeTokenAsync([FromBody] string? token)
        {
            var refreshToken = token ?? Request.Cookies["__Host-refreshToken"];
            var result = await _mediator.Send(new RevokeTokenCommand(refreshToken));
            Response.Cookies.Delete("__Host-refreshToken");
            return result.ToActionResult();
        }
        private void SetRefreshTokenInCookies(string refreshToken, DateTime expiresAt)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expiresAt,
                Path = "/"
            };

            Response.Cookies.Append(
                "__Host-refreshToken",
                refreshToken,
                cookieOptions);
        }
    }
}