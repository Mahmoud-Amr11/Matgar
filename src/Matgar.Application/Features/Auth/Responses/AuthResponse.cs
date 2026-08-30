using System.Text.Json.Serialization;

namespace Matgar.Application.Features.Auth.Responses
{
    public sealed record AuthResponse(
      string UserId,
      string Email,
      string AccessToken,
      DateTime AccessTokenExpiresAt,
      [property: JsonIgnore] string RefreshToken,
      [property: JsonIgnore] DateTime RefreshTokenExpiresOn
  );
}
