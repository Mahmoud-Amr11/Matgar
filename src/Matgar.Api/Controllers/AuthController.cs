using Asp.Versioning;
using Matgar.Api.Common;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Auth.Commands.ChangePassword;
using Matgar.Application.Features.Auth.Commands.ConfirmEmail;
using Matgar.Application.Features.Auth.Commands.Login;
using Matgar.Application.Features.Auth.Commands.Logout;
using Matgar.Application.Features.Auth.Commands.RefreshToken;
using Matgar.Application.Features.Auth.Commands.Register;
using Matgar.Application.Features.Auth.Commands.RevokeToken;
using MediatR;
using Microsoft.AspNetCore.Authorization;
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
                SetRefreshTokenInCookies(result.Value.RefreshToken, result.Value.RefreshTokenExpiresOn);
            }
            return result.ToActionResult();
        }

        [HttpPost("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string userId, string token, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new ConfirmEmailCommand(userId, token), cancellationToken);
            return result.ToActionResult();
        }



        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken()
        {
            var refreshToken = Request.Cookies["__Host-refreshToken"];

            if (refreshToken is null)
                return Result.Failure(Error.Unauthorized(message: "Invalid Token")).ToActionResult();

            var result = await _mediator.Send(new RefreshTokenCommand(refreshToken));
            if (result.IsSuccess)
            {
                SetRefreshTokenInCookies(
                    result.Value.RefreshToken,
                    result.Value.RefreshTokenExpiresOn);
            }
            return result.ToActionResult();
        }


        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeTokenAsync([FromBody] string? token)
        {
            var refreshToken = token ?? Request.Cookies["__Host-refreshToken"];
            var result = await _mediator.Send(new RevokeTokenCommand(refreshToken));
            Response.Cookies.Delete("__Host-refreshToken");
            return result.ToActionResult();
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword(ChangePasswordCommand request)
        {
            var result = await _mediator.Send(request);
            return result.ToActionResult();
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout(CancellationToken cancellationToken)
        {

            var result = await _mediator.Send(new LogoutCommand(), cancellationToken);
            Response.Cookies.Delete("__Host-refreshToken", new CookieOptions { Path = "/" });
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