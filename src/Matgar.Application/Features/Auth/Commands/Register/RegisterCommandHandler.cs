using Matgar.Application.Abstractions.Authentication;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs;
using MediatR;

namespace Matgar.Application.Features.Auth.Commands.Register
{
    public class RegisterCommandHandler : IRequestHandler<RegisterCommand, Result>
    {
        private readonly IIdentityService _identityService;
        private readonly IUnitOfWork _unitOfWork;
        public RegisterCommandHandler(IIdentityService identityService, IUnitOfWork unitOfWork)
        {
            _identityService = identityService;
            _unitOfWork = unitOfWork;
        }

        public async Task<Result> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            await _unitOfWork.BeginTransactionAsync(cancellationToken);

            var creatUserResult = await _identityService.CreateUserAsync(new UserDto(request.FirstName, request.LastName, request.Email, request.Password));

            if (!creatUserResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return creatUserResult;
            }

            var addRoleResult = await _identityService.AddToRoleAsync(request.Email, request.UserType.ToString());

            if (!addRoleResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return addRoleResult;
            }

            await _unitOfWork.CommitTransactionAsync(cancellationToken);
            var confirmEmailResult = await _identityService.GenerateEmailConfirmationTokenAsync(request.Email);


            return Result.Success;
        }
    }
}
