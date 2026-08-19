namespace Matgar.Application.DTOs.Authentication
{
    public sealed record RefreshTokenResult(string Token, DateTime ExpiresAt);
}
