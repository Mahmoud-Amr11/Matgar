namespace Matgar.Application.Features.Auth.AuthenticationDtos
{
    public sealed record UserDto(string FirstName, string LastName, string Email, string Password);
}
