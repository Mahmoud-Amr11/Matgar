using Matgar.Application.Abstractions.Repositories;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Payments.Commands.HandleWebhook;
using MediatR;

namespace Matgar.Application.Features.Payments.Handlers
{
    public class HandlePaymentWebhookHandler : IRequestHandler<HandlePaymentWebhookCommand, Result<bool>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public HandlePaymentWebhookHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Result<bool>> Handle(HandlePaymentWebhookCommand request, CancellationToken cancellationToken)
        {
            // Idempotent processing: find by TransactionReference
            var existing = await _unitOfWork.Payments.FindAsync(p => p.TransactionReference == request.TransactionReference, cancellationToken: cancellationToken);
            var payment = existing?.FirstOrDefault();

            if (payment == null)
            {
                // Unknown transaction - create record to avoid reprocessing
                try
                {
                    var newPay = new Domain.Entities.Payment
                    {
                        OrderId = Guid.Empty,
                        Amount = 0,
                        TransactionReference = request.TransactionReference,
                        Status = request.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ? PaymentStatus.Succeeded : PaymentStatus.Failed,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _unitOfWork.Payments.AddAsync(newPay, cancellationToken);
                    await _unitOfWork.SaveChangesAsync(cancellationToken);
                }
                catch
                {
                    // ignore
                }

                return Result<bool>.Success(true);
            }

            // If already processed, do nothing
            if (payment.Status == PaymentStatus.Succeeded || payment.Status == PaymentStatus.Failed)
                return Result<bool>.Success(true);

            // Update status
            payment.Status = request.Status.Equals("success", StringComparison.OrdinalIgnoreCase) ? PaymentStatus.Succeeded : PaymentStatus.Failed;
            _unitOfWork.Payments.Update(payment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return Result<bool>.Success(true);
        }
    }
}
