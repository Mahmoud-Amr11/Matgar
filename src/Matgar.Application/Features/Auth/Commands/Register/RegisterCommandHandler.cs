using Matgar.Application.Abstractions.Authentication;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs;
using Matgar.Application.Events;
using Matgar.Domain.Entities;
using MediatR;
using System.Text.Json;

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

            var createUserResult = await _identityService.CreateUserAsync(
                new UserDto(request.FirstName, request.LastName, request.Email, request.Password));

            if (!createUserResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(createUserResult.Errors);
            }

            var userId = createUserResult.Value;

            var addRoleResult = await _identityService.AddToRoleAsync(request.Email, request.UserType.ToString());
            if (!addRoleResult.IsSuccess)
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                return Result.Failure(addRoleResult.Errors);
            }

            var confirmEmailResult = await _identityService.GenerateEmailConfirmationTokenAsync(request.Email);


            var outboxMessage = new OutboxMessage
            {
                Type = nameof(UserRegisteredEvent),
                Content = JsonSerializer.Serialize(new UserRegisteredEvent(
                    userId, request.Email, confirmEmailResult))
            };

            await _unitOfWork.OutboxMessages.AddAsync(outboxMessage);

            await _unitOfWork.CommitTransactionAsync(cancellationToken);

            return Result.Success;
        }
    }
}
