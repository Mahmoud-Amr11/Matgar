using Matgar.Application.Common.Results;
using Matgar.Application.DTOs;

namespace Matgar.Application.Abstractions.Authentication
{
    public interface IIdentityService
    {
        Task<Result<string>> CreateUserAsync(UserDto userDto);
        Task<Result> AddToRoleAsync(string userEmail, string role);
        Task<string> GenerateEmailConfirmationTokenAsync(string userEmail);
    }
}
