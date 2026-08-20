using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Logout
{
    public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Result>
    {
        private readonly IRefreshTokenService _tokenService;
        private readonly ICurrentUserService _currentUserService;
        public LogoutCommandHandler(IRefreshTokenService tokenService, ICurrentUserService currentUserService)
        {
            _tokenService = tokenService;
            _currentUserService = currentUserService;
        }

        public async Task<Result> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return Error.Forbidden();

            await _tokenService.RevokeAllActiveTokensAsync(userId, cancellationToken);
            return Result.Success;
        }
    }

}
