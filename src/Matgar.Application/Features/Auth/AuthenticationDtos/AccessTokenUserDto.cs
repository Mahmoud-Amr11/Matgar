using System.Security.Claims;

namespace Matgar.Application.Features.Auth.AuthenticationDtos
{
    public sealed record AccessTokenUserDto(
       string UserId,
       string Email,
       IList<string> Roles,
       IList<Claim> AdditionalClaims);
}
