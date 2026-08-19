using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.RevokeToken
{
    public class RevokeTokenCommandHandler : IRequestHandler<RevokeTokenCommand, Result>
    {
        private readonly IRefreshTokenService _tokenService;

        public RevokeTokenCommandHandler(IRefreshTokenService tokenService)
        {
            _tokenService = tokenService;
        }

        public async Task<Result> Handle(RevokeTokenCommand request, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(request.token))
                return Error.Unauthorized("InvalidCredentials", "Invalid authentication credentials");


            return await _tokenService.RevokeTokenAsync(request.token, cancellationToken);
        }
    }
}
