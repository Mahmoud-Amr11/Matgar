using Matgar.Application.Features.Payments.PaymentDtos;

namespace Matgar.Application.Abstractions.Services
{
    public interface IPaymentGateway
    {
        Task<PaymentResult> CreatePaymentAsync(
       PaymentRequest request,
       CancellationToken cancellationToken);

        Task<PaymentWebhookResult?> VerifyWebhookAsync(
       string payload, string hmac, CancellationToken cancellationToken);
    }
}
