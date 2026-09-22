namespace Matgar.Application.Features.Auth.AuthenticationDtos
{
    public sealed record RefreshTokenRotationResult(
       string UserId,
       string NewRefreshToken,
       DateTime NewRefreshTokenExpiresOn);
}
