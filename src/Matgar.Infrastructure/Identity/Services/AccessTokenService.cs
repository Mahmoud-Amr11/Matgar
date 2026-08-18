using Matgar.Application.Abstractions.Identity;
using Matgar.Application.DTOs.Authentication;
using Matgar.Infrastructure.Options;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Matgar.Infrastructure.Identity.Services
{
    internal class AccessTokenService : IAccessTokenService
    {
        private readonly JwtOptions _jwtOptions;
        public AccessTokenService(IOptions<JwtOptions> options)
        {
            _jwtOptions = options.Value;
        }
        public AccessTokenResult GenerateAccessToken(AccessTokenUserDto user)
        {
            var claims = new List<Claim> {
                new (JwtRegisteredClaimNames.Sub,user.UserId),
                new(JwtRegisteredClaimNames.Email,user.Email),
                new(JwtRegisteredClaimNames.Jti,Guid.NewGuid().ToString())
            };

            claims.AddRange(user.AdditionalClaims);

            claims.AddRange(user.Roles.Select(role => new Claim(ClaimTypes.Role, role)));

            var expiresAt = DateTime.Now.AddMinutes(_jwtOptions.AccessTokenDurationMinutes);
            var key = new SymmetricSecurityKey(
             Encoding.UTF8.GetBytes(_jwtOptions.Key));

            var signingCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _jwtOptions.Issuer,
                audience: _jwtOptions.Audience,
                claims: claims,
                expires: expiresAt,
                  signingCredentials: signingCredentials
                );


            return new AccessTokenResult(new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
        }
    }
}
