namespace Matgar.Application.DTOs.Authentication
{
    public sealed record RefreshTokenRotationResult(
       string UserId,
       string NewRefreshToken,
       DateTime NewRefreshTokenExpiresOn);
}
