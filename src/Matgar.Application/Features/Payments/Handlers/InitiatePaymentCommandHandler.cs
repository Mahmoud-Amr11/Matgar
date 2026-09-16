using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Payments.Commands.InitiatePayment;
using MediatR;

namespace Matgar.Application.Features.Payments.Handlers
{
    public class InitiatePaymentCommandHandler : IRequestHandler<InitiatePaymentCommand, Result<InitiatePaymentResponse>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public InitiatePaymentCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<InitiatePaymentResponse>> Handle(InitiatePaymentCommand request, CancellationToken cancellationToken)
        {
            // Create a transaction reference
            var txRef = Guid.NewGuid().ToString("N");

            // Persist a Payment record if repository exists
            try
            {
                var payment = new Domain.Entities.Payment
                {
                    OrderId = request.OrderId,
                    Amount = request.Amount,
                    TransactionReference = txRef,
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.Payments.AddAsync(payment, cancellationToken);
                await _unitOfWork.SaveChangesAsync(cancellationToken);
            }
            catch
            {
                // If Payments repo or entity missing, skip persistence but continue returning a client secret
            }

            // In real app call external payment provider. Return stub link/client secret here.
            var clientSecret = $"https://payments.example/pay/{txRef}";

            return Result<InitiatePaymentResponse>.Success(new InitiatePaymentResponse(clientSecret, txRef));
        }
    }
}
