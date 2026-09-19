using System.Linq.Expressions;
using Matgar.Application.Features.Payments.Commands.HandleWebhook;
using Matgar.Application.Features.Payments.Handlers;

namespace Matgar.Application.Tests.Handlers;

public class HandlePaymentWebhookHandlerTests
{
    private readonly string _txRef = Guid.NewGuid().ToString("N");
    private readonly Mock<IGenericRepository<Payment>> _payments = new();

    private HandlePaymentWebhookHandler CreateHandler()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Payments).Returns(_payments.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return new HandlePaymentWebhookHandler(unitOfWork.Object);
    }

    private void PaymentsAre(params Payment[] payments)
    {
        _payments.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Payment, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(payments.ToList());
    }

    [Fact]
    public async Task Handle_should_register_unknown_transaction_as_succeeded()
    {
        PaymentsAre();

        Payment? captured = null;
        _payments.Setup(r => r.AddAsync(It.IsAny<Payment>(), It.IsAny<CancellationToken>()))
            .Callback<Payment, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(
            new HandlePaymentWebhookCommand(_txRef, "success", "{}"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.TransactionReference.Should().Be(_txRef);
        captured.Status.Should().Be(PaymentStatus.Succeeded);
    }

    [Fact]
    public async Task Handle_should_mark_failure_for_unknown_transaction()
    {
        PaymentsAre();

        var result = await CreateHandler().Handle(
            new HandlePaymentWebhookCommand(_txRef, "failed", "{}"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
    }

    [Fact]
    public async Task Handle_should_update_pending_payment_to_succeeded()
    {
        var payment = new Payment { TransactionReference = _txRef, Status = PaymentStatus.Pending };
        PaymentsAre(payment);

        var result = await CreateHandler().Handle(
            new HandlePaymentWebhookCommand(_txRef, "success", "{}"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        _payments.Verify(r => r.Update(payment), Times.Once);
    }

    [Fact]
    public async Task Handle_should_be_idempotent_when_payment_already_processed()
    {
        var payment = new Payment { TransactionReference = _txRef, Status = PaymentStatus.Succeeded };
        PaymentsAre(payment);

        var result = await CreateHandler().Handle(
            new HandlePaymentWebhookCommand(_txRef, "success", "{}"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        payment.Status.Should().Be(PaymentStatus.Succeeded);
        _payments.Verify(r => r.Update(It.IsAny<Payment>()), Times.Never);
    }

    [Fact]
    public async Task Handle_should_not_downgrade_succeeded_payment_to_failed()
    {
        var payment = new Payment { TransactionReference = _txRef, Status = PaymentStatus.Succeeded };
        PaymentsAre(payment);

        await CreateHandler().Handle(
            new HandlePaymentWebhookCommand(_txRef, "failed", "{}"),
            CancellationToken.None);

        payment.Status.Should().Be(PaymentStatus.Succeeded);
        _payments.Verify(r => r.Update(It.IsAny<Payment>()), Times.Never);
    }
}