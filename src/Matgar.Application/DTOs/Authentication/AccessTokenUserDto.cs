using System.Security.Claims;

namespace Matgar.Application.DTOs.Authentication
{
    public sealed record AccessTokenUserDto(string UserId,
       string Email,
       IList<string> Roles,
       IList<Claim> AdditionalClaims);
}
