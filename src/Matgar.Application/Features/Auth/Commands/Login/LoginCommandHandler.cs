using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Auth.Responses;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<AuthResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IRefreshTokenService _refreshTokenService;
        public LoginCommandHandler(IIdentityService identityService, IAccessTokenService accessTokenService, IRefreshTokenService refreshTokenService)
        {
            _identityService = identityService;
            _accessTokenService = accessTokenService;
            _refreshTokenService = refreshTokenService;
        }

        public async Task<Result<AuthResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var validatUser =
            await _identityService.ValidateCredentialsAsync(
                request.Email,
                request.Password);

            if (!validatUser.IsSuccess)
            {
                return Result<AuthResponse>.Failure(
                    validatUser.Errors);
            }

            var accessToken =
                _accessTokenService.GenerateAccessToken(
                    validatUser.Value);

            var refreshToken =
                await _refreshTokenService
                    .GenerateAndStoreRefreshTokenAsync(
                        validatUser.Value.UserId,
                        cancellationToken);

            var response = new AuthResponse(
                validatUser.Value.UserId,
                validatUser.Value.Email,
                accessToken.Token,
                accessToken.ExpiresAt,
                refreshToken.Token,
                refreshToken.ExpiresAt);

            return response;
        }
    }

}
