using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Results;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.ChangePassword
{
    public class ChangePasswordCommandHandler : IRequestHandler<ChangePasswordCommand, Result>
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IIdentityService _identityService;
        public ChangePasswordCommandHandler(ICurrentUserService currentUserService, IIdentityService identityService)
        {
            _currentUserService = currentUserService;
            _identityService = identityService;
        }

        public async Task<Result> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _currentUserService.UserId;

            var changePasswordResult = await _identityService.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);

            return changePasswordResult;
        }
    }
}
