using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Login
{
    public class LoginCommandHandler : IRequestHandler<LoginCommand, Result<LoginResponse>>
    {
        private readonly IIdentityService _identityService;
        private readonly IAccessTokenService _accessTokenService;

        public LoginCommandHandler(IIdentityService identityService, IAccessTokenService accessTokenService)
        {
            _identityService = identityService;
            _accessTokenService = accessTokenService;
        }

        public async Task<Result<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var validatUser = await _identityService.ValidateCredentialsAsync(request.Email, request.Password);
            if (!validatUser.IsSuccess)
                return Result<LoginResponse>.Failure(validatUser.Errors);


            var accessToken = _accessTokenService.GenerateAccessToken(validatUser.Value);


            return new LoginResponse(accessToken.Token, null, accessToken.ExpiresAt);
        }
    }

}
