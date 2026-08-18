namespace Matgar.Application.DTOs.Authentication
{
    public sealed record AccessTokenResult(string Token, DateTime ExpiresAt);
}
