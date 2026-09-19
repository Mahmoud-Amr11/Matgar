using System.Linq.Expressions;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Features.Orders.Commands.UpdateVendorOrderStatus;

namespace Matgar.Application.Tests.Handlers;

public class UpdateVendorOrderStatusCommandHandlerTests
{
    private readonly Guid _vendorId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Mock<IGenericRepository<Order>> _orders = new();
    private readonly Mock<IGenericRepository<OrderItem>> _orderItems = new();
    private readonly Mock<IProductVariantRepository> _variants = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IGenericRepository<OutboxMessage>> _outbox = new();

    private Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Orders).Returns(_orders.Object);
        unitOfWork.Setup(u => u.OrderItems).Returns(_orderItems.Object);
        unitOfWork.Setup(u => u.ProductVariants).Returns(_variants.Object);
        unitOfWork.Setup(u => u.Products).Returns(_products.Object);
        unitOfWork.Setup(u => u.OutboxMessages).Returns(_outbox.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return unitOfWork;
    }

    private UpdateVendorOrderStatusCommandHandler CreateHandler(Mock<IUnitOfWork> unitOfWork)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(_vendorId.ToString());
        return new UpdateVendorOrderStatusCommandHandler(unitOfWork.Object, currentUser.Object);
    }

    private void SetupOrderOwnedByVendor(Order order)
    {
        _orders.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order });

        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem>
            {
                new() { OrderId = _orderId, ProductVariantId = _variantId }
            });

        _variants.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductVariant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductVariant>
            {
                new() { Id = _variantId, ProductId = _productId }
            });

        _products.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
    }

    private OutboxMessage? CaptureOutbox()
    {
        OutboxMessage? outbox = null;
        _outbox.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, CancellationToken>((m, _) => outbox = m)
            .Returns(Task.CompletedTask);
        return outbox;
    }

    [Fact]
    public async Task Handle_should_confirm_pending_order()
    {
        var order = new Order { Id = _orderId, Status = OrderStatus.Pending };
        SetupOrderOwnedByVendor(order);
        CaptureOutbox();

        var result = await CreateHandler(CreateUnitOfWork()).Handle(
            new UpdateVendorOrderStatusCommand(_orderId, (int)OrderStatus.Confirmed),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Confirmed);
        order.ConfirmedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_should_ship_confirmed_order()
    {
        var order = new Order { Id = _orderId, Status = OrderStatus.Confirmed };
        SetupOrderOwnedByVendor(order);

        OutboxMessage? outbox = null;
        _outbox.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, CancellationToken>((m, _) => outbox = m)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler(CreateUnitOfWork()).Handle(
            new UpdateVendorOrderStatusCommand(_orderId, (int)OrderStatus.Shipped),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        order.Status.Should().Be(OrderStatus.Shipped);
        order.ShippedAt.Should().NotBeNull();
        outbox.Should().NotBeNull();
        outbox!.Type.Should().Be("OrderStatusUpdated");
    }

    [Fact]
    public async Task Handle_should_fail_on_invalid_transition()
    {
        var order = new Order { Id = _orderId, Status = OrderStatus.Pending };
        SetupOrderOwnedByVendor(order);

        // Pending -> Shipped is not a valid transition (must go through Confirmed).
        var result = await CreateHandler(CreateUnitOfWork()).Handle(
            new UpdateVendorOrderStatusCommand(_orderId, (int)OrderStatus.Shipped),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Order.InvalidTransition");
        order.Status.Should().Be(OrderStatus.Pending);
    }

    [Fact]
    public async Task Handle_should_fail_when_order_not_found()
    {
        _orders.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order>());

        var result = await CreateHandler(CreateUnitOfWork()).Handle(
            new UpdateVendorOrderStatusCommand(_orderId, (int)OrderStatus.Confirmed),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Order.NotFound");
    }

    [Fact]
    public async Task Handle_should_forbid_vendor_without_products_in_order()
    {
        var order = new Order { Id = _orderId, Status = OrderStatus.Pending };
        _orders.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Order, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Order> { order });

        _orderItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<OrderItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<OrderItem>
            {
                new() { OrderId = _orderId, ProductVariantId = _variantId }
            });

        _variants.Setup(r => r.FindAsync(It.IsAny<Expression<Func<ProductVariant, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<ProductVariant>
            {
                new() { Id = _variantId, ProductId = _productId }
            });

        _products.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Product, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        var result = await CreateHandler(CreateUnitOfWork()).Handle(
            new UpdateVendorOrderStatusCommand(_orderId, (int)OrderStatus.Confirmed),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Order.Forbidden");
    }
}