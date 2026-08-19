namespace Matgar.Application.Features.Auth.Responses
{
    public sealed record AuthResponse(
       string UserId,
        string Email,
        string AccessToken,
        DateTime AccessTokenExpiresAt,
        string RefreshToken,
        DateTime RefreshTokenExpiresOn
 );
}
