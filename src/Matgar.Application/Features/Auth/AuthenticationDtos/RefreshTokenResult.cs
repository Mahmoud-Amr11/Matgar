namespace Matgar.Application.Features.Auth.AuthenticationDtos
{
    public sealed record RefreshTokenResult(string Token, DateTime ExpiresAt);
}
