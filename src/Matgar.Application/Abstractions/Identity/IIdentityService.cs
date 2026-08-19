using Matgar.Application.Common.Results;
using Matgar.Application.DTOs.Authentication;

namespace Matgar.Application.Abstractions.Identity
{
    public interface IIdentityService
    {

        Task<Result<string>> CreateUserAsync(UserDto userDto);
        Task<Result> AddToRoleAsync(string userEmail, string role);
        Task<string> GenerateEmailConfirmationTokenAsync(string userEmail);
        Task<Result> ConfirmEmailAsync(string userId, string Token);

        Task<Result<AccessTokenUserDto>> ValidateCredentialsAsync(string email, string password);
        Task<Result<AccessTokenUserDto>> GetUserAsync(string userId);
    }
}
