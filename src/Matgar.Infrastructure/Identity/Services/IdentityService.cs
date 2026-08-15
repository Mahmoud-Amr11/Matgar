using Matgar.Application.Abstractions.Authentication;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs;
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

        public async Task<Result> AddToRoleAsync(string userEmail, string role)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
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

        public async Task<string> GenerateEmailConfirmationTokenAsync(string userEmail)
        {
            var user = await _userManager.FindByEmailAsync(userEmail);
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            return WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
        }
    }
}
