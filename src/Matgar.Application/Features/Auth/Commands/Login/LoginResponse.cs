namespace Matgar.Application.Features.Auth.Commands.Login
{
    public sealed record LoginResponse(string Token, string RefreshToken, DateTime Expiration);
}
