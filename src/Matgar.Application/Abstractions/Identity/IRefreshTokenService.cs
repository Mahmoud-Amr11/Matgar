using Matgar.Application.Common.Results;
using Matgar.Application.DTOs.Authentication;

namespace Matgar.Application.Abstractions.Identity
{
    public interface IRefreshTokenService
    {
        Task<RefreshTokenResult> GenerateAndStoreRefreshTokenAsync(string userId, CancellationToken cancellationToken);
        Task<Result<RefreshTokenRotationResult>> RotateRefreshTokenAsync(string token, CancellationToken cancellationToken);
        Task RevokeAllActiveTokensAsync(string userId, CancellationToken cancellationToken);
        Task<Result> RevokeTokenAsync(string token, CancellationToken cancellationToken);
    }
}
