using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Abstractions.Services;
using Matgar.Application.DTOs.Payments;
using Matgar.Application.Features.Payments.Commands.HandlePaymobWebhook;
using Microsoft.Extensions.Logging;

namespace Matgar.Application.Tests.Handlers;

public class HandlePaymentWebhookHandlerTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IPaymentGateway> _gateway = new();
    private readonly Mock<IProcessedWebhookRepository> _webhooks = new();
    private readonly Mock<IOrderRepository> _orders = new();

    private HandlePaymobWebhookCommandHandler CreateHandler()
    {
        _unitOfWork.Setup(u => u.ProcessedWebhooks).Returns(_webhooks.Object);
        _unitOfWork.Setup(u => u.Orders).Returns(_orders.Object);
        _unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new HandlePaymobWebhookCommandHandler(
            _gateway.Object,
            _unitOfWork.Object,
            new Mock<ILogger<HandlePaymobWebhookCommandHandler>>().Object);
    }

    private void GatewayReturns(PaymentWebhookResult? result)
        => _gateway.Setup(g => g.VerifyWebhookAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(result);

    private void WebhookRegistered(bool registered)
        => _webhooks.Setup(r => r.TryRegisterAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(registered);

    private void OrderFound(Order? order)
        => _orders.Setup(r => r.GetByReferenceAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(order);

    private static Order OrderWithTotal(decimal total) => new() { Id = Guid.NewGuid(), TotalAmount = total };

    private static PaymentWebhookResult SuccessResult(string reference, long amountCents = 10000)
        => new()
        {
            TransactionId = 98765,
            OrderReference = reference,
            AmountCents = amountCents,
            Success = true
        };

    [Fact]
    public async Task Handle_should_reject_webhook_with_invalid_signature()
    {
        GatewayReturns(null);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "bad-hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.InvalidSignature);
        _webhooks.Verify(r => r.TryRegisterAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_should_be_idempotent_for_duplicate_events()
    {
        GatewayReturns(SuccessResult("order-ref"));
        WebhookRegistered(false);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.Duplicate);
    }

    [Fact]
    public async Task Handle_should_ignore_webhook_for_unknown_order()
    {
        GatewayReturns(SuccessResult("unknown-ref"));
        WebhookRegistered(true);
        OrderFound(null);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.Ignored);
    }

    [Fact]
    public async Task Handle_should_mark_order_paid_on_success()
    {
        var order = OrderWithTotal(100m);
        GatewayReturns(SuccessResult(order.Reference));
        WebhookRegistered(true);
        OrderFound(order);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.Processed);
        order.IsPaid.Should().BeTrue();
        order.Payment.Should().NotBeNull();
        order.Payment!.Status.Should().Be(PaymentStatus.Succeeded);
        order.Payment.TransactionReference.Should().Be("98765");
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_ignore_success_webhook_with_amount_mismatch()
    {
        var order = OrderWithTotal(100m);
        GatewayReturns(SuccessResult(order.Reference, amountCents: 9500));
        WebhookRegistered(true);
        OrderFound(order);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.Ignored);
        order.Payment.Should().BeNull();
    }

    [Fact]
    public async Task Handle_should_mark_payment_failed_when_not_success_and_not_pending()
    {
        var order = OrderWithTotal(100m);
        GatewayReturns(new PaymentWebhookResult { TransactionId = 111, OrderReference = order.Reference, Success = false, Pending = false });
        WebhookRegistered(true);
        OrderFound(order);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.Processed);
        order.Payment.Should().NotBeNull();
        order.Payment!.Status.Should().Be(PaymentStatus.Failed);
    }

    [Fact]
    public async Task Handle_should_mark_order_payment_as_refunded()
    {
        var order = OrderWithTotal(100m);
        GatewayReturns(new PaymentWebhookResult { TransactionId = 222, OrderReference = order.Reference, IsRefunded = true });
        WebhookRegistered(true);
        OrderFound(order);

        var result = await CreateHandler().Handle(
            new HandlePaymobWebhookCommand("{}", "hmac"),
            CancellationToken.None);

        result.Should().Be(WebhookOutcome.Processed);
        order.Payment.Should().NotBeNull();
        order.Payment!.Status.Should().Be(PaymentStatus.Refunded);
    }
}