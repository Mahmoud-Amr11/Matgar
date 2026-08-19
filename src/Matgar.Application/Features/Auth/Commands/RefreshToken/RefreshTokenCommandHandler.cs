using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Auth.Responses;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.RefreshToken
{
    public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, Result<AuthResponse>>
    {
        private readonly IRefreshTokenService _refreshTokenService;
        private readonly IAccessTokenService _accessTokenService;
        private readonly IIdentityService _identityService;
        public RefreshTokenCommandHandler(IRefreshTokenService refreshTokenService, IAccessTokenService accessTokenService, IIdentityService identityService)
        {
            _refreshTokenService = refreshTokenService;
            _accessTokenService = accessTokenService;
            _identityService = identityService;
        }

        public async Task<Result<AuthResponse>> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
        {
            var rotationResult = await _refreshTokenService.RotateRefreshTokenAsync(request.Token, cancellationToken);

            if (!rotationResult.IsSuccess)
                return Result<AuthResponse>.Failure(rotationResult.Errors);

            var user = await _identityService.GetUserAsync(rotationResult.Value.UserId);

            var accessToken = _accessTokenService.GenerateAccessToken(user.Value);
            return new AuthResponse(user.Value.UserId, user.Value.Email, accessToken.Token, accessToken.ExpiresAt, rotationResult.Value.NewRefreshToken,
               rotationResult.Value.NewRefreshTokenExpiresOn);

        }
    }
}
