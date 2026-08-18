using Matgar.Application.DTOs.Authentication;

namespace Matgar.Application.Abstractions.Identity
{
    public interface IAccessTokenService
    {
        AccessTokenResult GenerateAccessToken(AccessTokenUserDto user);
    }
}
