using System.Linq.Expressions;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Features.Orders.Commands.CancelOrder;

namespace Matgar.Application.Tests.Handlers;

public class CancelOrderCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Mock<IGenericRepository<Order>> _orders = new();
    private readonly Mock<IGenericRepository<OrderItem>> _orderItems = new();
    private readonly Mock<IGenericRepository<StockItem>> _stockItems = new();
    private readonly Mock<IGenericRepository<OutboxMessage>> _outbox = new();

    private CancelOrderCommandHandler CreateHandler(out Mock<IUnitOfWork> unitOfWork)
    {
        unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Orders).Returns(_orders.Object);
        unitOfWork.Setup(u => u.OrderItems).Returns(_orderItems.Object);
        unitOfWork.Setup(u => u.StockItems).Returns(_stockItems.Object);
        unitOfWork.Setup(u => u.OutboxMessages).Returns(_outbox.Object);
        unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(_userId.ToString());

        return new CancelOrderCommandHandler(unitOfWork.Object, currentUser.Object);
    }

    private Order PendingOrder() => new()
    {
        Id = _orderId,
        CustomerId = _userId,
        Status = OrderStatus.Pending
    };

    private void OrderIs(Order order)
    {
        _orders.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order });
    }

    [Fact]
    public async Task Handle_should_cancel_order_and_release_reserved_stock()
    {
        var order = PendingOrder();
        OrderIs(order);

        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem>
            {
                new() { OrderId = _orderId, ProductVariantId = _variantId, Quantity = 2 }
            });

        var stock = new StockItem { ProductVariantId = _variantId, QuantityOnHand = 10, QuantityReserved = 5 };
        _stockItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StockItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockItem> { stock });

        OutboxMessage? outbox = null;
        _outbox.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, CancellationToken>((m, _) => outbox = m)
            .Returns(Task.CompletedTask);

        var role = CreateHandler(out var unitOfWork);
        var result = await role.Handle(new CancelOrderCommand(_orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Cancelled);
        order.CancelledAt.Should().NotBeNull();
        stock.QuantityReserved.Should().Be(3);
        outbox.Should().NotBeNull();
        outbox!.Type.Should().Be("OrderCancelled");
        unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_never_make_reserved_negative()
    {
        var order = PendingOrder();
        OrderIs(order);

        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem>
            {
                new() { OrderId = _orderId, ProductVariantId = _variantId, Quantity = 2 }
            });

        var stock = new StockItem { ProductVariantId = _variantId, QuantityReserved = 1 };
        _stockItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StockItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockItem> { stock });

        var handler = CreateHandler(out _);
        var result = await handler.Handle(new CancelOrderCommand(_orderId), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        stock.QuantityReserved.Should().Be(0);
    }

    [Fact]
    public async Task Handle_should_fail_when_order_not_found_for_user()
    {
        _orders.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var result = await CreateHandler(out _).Handle(new CancelOrderCommand(_orderId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Order.NotFound");
    }

    [Theory]
    [InlineData(OrderStatus.Shipped)]
    [InlineData(OrderStatus.Delivered)]
    public async Task Handle_should_fail_when_order_already_shipped_or_delivered(OrderStatus status)
    {
        var order = PendingOrder();
        order.Status = status;
        OrderIs(order);

        var result = await CreateHandler(out _).Handle(new CancelOrderCommand(_orderId), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Order.CannotCancel");
    }
}