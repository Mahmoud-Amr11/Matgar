using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Enums;
using Matgar.Application.Common.Results;
using Matgar.Application.DTOs.Authentication;
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

            try
            {
                var createUserResult = await _identityService.CreateUserAsync(
                    new UserDto(request.FirstName, request.LastName, request.Email, request.Password));

                if (!createUserResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<string>.Failure(createUserResult.Errors);
                }

                var userId = createUserResult.Value;

                var addRoleResult = await _identityService.AddToRoleAsync(userId, UserType.Customer.Value);
                if (!addRoleResult.IsSuccess)
                {
                    await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                    return Result<string>.Failure(addRoleResult.Errors);
                }

                var confirmEmailToken = await _identityService.GenerateEmailConfirmationTokenAsync(userId);

                var outboxMessage = new OutboxMessage
                {
                    Type = nameof(UserRegisteredEvent),
                    Content = JsonSerializer.Serialize(new UserRegisteredEvent(userId, request.Email, confirmEmailToken))
                };

                await _unitOfWork.OutboxMessages.AddAsync(outboxMessage);
                await _unitOfWork.CommitTransactionAsync(cancellationToken);
                return Result.Success;
            }
            catch
            {
                await _unitOfWork.RollbackTransactionAsync(cancellationToken);
                throw;
            }
        }
    }
}

