using Matgar.Application.Features.Auth.AuthenticationDtos;

namespace Matgar.Application.Abstractions.Identity
{
    public interface IAccessTokenService
    {
        AccessTokenResult GenerateAccessToken(AccessTokenUserDto user);
    }
}
