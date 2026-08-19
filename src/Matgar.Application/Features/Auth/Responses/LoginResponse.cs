namespace Matgar.Application.Features.Auth.Responses
{
    public sealed record LoginResponse(
   string UserId,
  string Email,
  string AccessToken,
  DateTime AccessTokenExpiresOn);
}
