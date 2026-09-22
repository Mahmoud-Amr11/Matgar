namespace Matgar.Application.Features.Auth.AuthenticationDtos
{
    public sealed record AccessTokenResult(string Token, DateTime ExpiresAt);
}
