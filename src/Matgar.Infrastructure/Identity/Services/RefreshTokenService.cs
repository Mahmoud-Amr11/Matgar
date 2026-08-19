using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs.Authentication;
using Matgar.Infrastructure.Identity.Entities;
using Matgar.Infrastructure.Options;
using Matgar.Infrastructure.Persistence.Contexts;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Security.Cryptography;
using System.Text;

namespace Matgar.Infrastructure.Identity.Services
{
    internal class RefreshTokenService : IRefreshTokenService
    {
        private readonly ApplicationDbContext _context;
        private readonly JwtOptions _jwtOptions;
        public RefreshTokenService(ApplicationDbContext context, IOptions<JwtOptions> jwtOptions)
        {
            _context = context;
            _jwtOptions = jwtOptions.Value;
        }

        public async Task<RefreshTokenResult> GenerateAndStoreRefreshTokenAsync(string userId, CancellationToken cancellationToken)
        {
            var token = CreateToken();

            var hashToken = HashToken(token);

            var expiresAt = DateTime.Now.AddDays(
                _jwtOptions.RefreshTokenDurationDays);

            var refreshToken = new RefreshToken
            {
                Id = Guid.NewGuid(),

                HashToken = hashToken,

                UserId = userId,

                CreatedOn = DateTime.Now,

                ExpiresOn = expiresAt
            };

            _context.RefreshTokens.Add(refreshToken);

            await _context.SaveChangesAsync(cancellationToken);

            return new RefreshTokenResult(
                token,
                expiresAt);
        }


        public async Task<Result<RefreshTokenRotationResult>> RotateRefreshTokenAsync(string token, CancellationToken cancellationToken)
        {
            var hashToken = HashToken(token);
            var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.HashToken == hashToken, cancellationToken);

            if (oldToken is null)
                return Error.Unauthorized(message: "Invalid refreshtoken");

            if (oldToken.RevokedOn is not null)
            {
                await RevokeAllActiveTokensAsync(oldToken.UserId, cancellationToken);

                return Error.Unauthorized(
                    message: "Invalid refresh token");
            }


            if (oldToken.IsExpired)
                return Error.Unauthorized(message: "Invalid refresh token");

            oldToken.RevokedOn = DateTime.Now;

            var newToken = await GenerateAndStoreRefreshTokenAsync(oldToken.UserId, cancellationToken);
            return new RefreshTokenRotationResult(oldToken.UserId, newToken.Token, newToken.ExpiresAt);

        }

        public async Task RevokeAllActiveTokensAsync(string userId, CancellationToken cancellationToken)
        {
            var activeTokens = await _context.RefreshTokens.Where(t => t.UserId == userId && t.RevokedOn == null)
                .ToListAsync(cancellationToken);


            if (activeTokens.Any())
            {
                foreach (var activeToken in activeTokens)
                    activeToken.RevokedOn = DateTime.Now;
            }


            await _context.SaveChangesAsync(cancellationToken);

        }

        public async Task<Result> RevokeTokenAsync(string token, CancellationToken cancellationToken)
        {
            var hashedtoken = HashToken(token);

            var oldToken = await _context.RefreshTokens.FirstOrDefaultAsync(t => t.HashToken == hashedtoken, cancellationToken);
            if (oldToken is null)
                return Error.Unauthorized(message: "Invalid token");

            if (oldToken.RevokedOn is not null)
                return Error.Unauthorized(message: "Invalid token");

            oldToken.RevokedOn = DateTime.UtcNow;

            await _context.SaveChangesAsync(cancellationToken);

            return Result.Success;
        }
        private static string CreateToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(64);

            return WebEncoders.Base64UrlEncode(bytes);
        }

        private static string HashToken(string token)
        {
            var bytes = Encoding.UTF8.GetBytes(token);

            var hash = SHA256.HashData(bytes);

            return Convert.ToHexString(hash);
        }


    }
}

