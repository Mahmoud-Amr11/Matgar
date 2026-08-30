using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs.Authentication;
using Matgar.Infrastructure.Identity.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using System.Text;

namespace Matgar.Infrastructure.Identity.Services
{
    internal class IdentityService : IIdentityService
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public IdentityService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }


        public async Task<Result<string>> CreateUserAsync(UserDto userDto)
        {
            var user = new ApplicationUser
            {
                FirstName = userDto.FirstName,
                LastName = userDto.LastName,
                Email = userDto.Email,
                UserName = userDto.Email
            };

            var result = await _userManager.CreateAsync(user, userDto.Password);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(IdentityErrorMapper.Map).ToList();
                return Result<string>.Failure(errors);
            }

            return user.Id;
        }

        public async Task<Result> AddToRoleAsync(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Error.NotFound("User not found");

            var result = await _userManager.AddToRoleAsync(user, role);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(IdentityErrorMapper.Map).ToList();
                return Result.Failure(errors);
            }

            return Result.Success;
        }

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }

        public async Task<Result> ConfirmEmailAsync(string userId, string token)
        {
            var user = await _userManager.FindByIdAsync(userId);

            if (user is null)
                return Error.NotFound(message: "User not found.");

            string decodedToken;
            try
            {
                var decodedTokenBytes = WebEncoders.Base64UrlDecode(token);
                decodedToken = Encoding.UTF8.GetString(decodedTokenBytes);
            }
            catch (FormatException)
            {
                return Error.Validation(
                    code: "InvalidToken",
                    message: "Confirmation link is invalid or expired");
            }

            var result = await _userManager.ConfirmEmailAsync(user, decodedToken);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(IdentityErrorMapper.Map).ToList();
                return Result.Failure(errors);
            }

            return Result.Success;

        }

        public async Task<Result<AccessTokenUserDto>> ValidateCredentialsAsync(string email, string password)
        {
            var user = await _userManager.FindByEmailAsync(email);
            if (user is null)
                return Error.Unauthorized(
                    message: "Email or Password is invalid");


            if (await _userManager.IsLockedOutAsync(user))
                return Error.Forbidden(
                    "Account locked");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);


            if (!isPasswordValid)
            {
                await _userManager.AccessFailedAsync(user);
                return Error.Unauthorized(
                    message: "Email or Password is invalid");
            }




            if (!await _userManager.IsEmailConfirmedAsync(user))
                return Error.Unauthorized(
                    message: "Please confirm your email");


            await _userManager.ResetAccessFailedCountAsync(user);


            return await BuildUserTokenInfoAsync(user);
        }
        public async Task<Result<AccessTokenUserDto>> GetUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null)
                return Error.NotFound(message: "User not found");

            return await BuildUserTokenInfoAsync(user);
        }



        private async Task<AccessTokenUserDto> BuildUserTokenInfoAsync(ApplicationUser user)
        {
            var roles = await _userManager.GetRolesAsync(user);
            var claims = await _userManager.GetClaimsAsync(user);

            return new AccessTokenUserDto(user.Id, user.Email!, roles, claims);
        }

        public async Task<Result> ChangePasswordAsync(string userId, string currentPassword, string newPassword)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user is null) return Error.Unauthorized();


            var result = await _userManager.ChangePasswordAsync(user, currentPassword, newPassword);

            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(IdentityErrorMapper.Map).ToList();
                return Result.Failure(errors);
            }

            return Result.Success;
        }
    }
}
