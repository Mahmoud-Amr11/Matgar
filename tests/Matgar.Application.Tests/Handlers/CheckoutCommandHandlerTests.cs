using System.Linq.Expressions;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Features.Orders.Commands.Checkout;

namespace Matgar.Application.Tests.Handlers;

public class CheckoutCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Mock<ICartRepository> _carts = new();
    private readonly Mock<IGenericRepository<StockItem>> _stockItems = new();
    private readonly Mock<ICouponRepository> _coupons = new();
    private readonly Mock<IGenericRepository<Address>> _addresses = new();
    private readonly Mock<IGenericRepository<Order>> _orders = new();
    private readonly Mock<IGenericRepository<OutboxMessage>> _outbox = new();

    private Mock<IUnitOfWork> CreateUnitOfWork()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Carts).Returns(_carts.Object);
        unitOfWork.Setup(u => u.StockItems).Returns(_stockItems.Object);
        unitOfWork.Setup(u => u.Coupons).Returns(_coupons.Object);
        unitOfWork.Setup(u => u.Addresses).Returns(_addresses.Object);
        unitOfWork.Setup(u => u.Orders).Returns(_orders.Object);
        unitOfWork.Setup(u => u.OutboxMessages).Returns(_outbox.Object);
        unitOfWork.Setup(u => u.BeginTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);
        return unitOfWork;
    }

    private CheckoutCommandHandler CreateHandler(Mock<IUnitOfWork> unitOfWork)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(_userId.ToString());
        return new CheckoutCommandHandler(unitOfWork.Object, currentUser.Object);
    }

    private Cart CartWithOneItem(int quantity, decimal price = 100m)
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
        cart.Items.Add(new CartItem
        {
            Id = Guid.NewGuid(),
            CartId = cart.Id,
            ProductVariantId = _variantId,
            Quantity = quantity,
            PriceSnapshot = price,
            ProductVariant = new ProductVariant { Id = _variantId, Sku = "SKU-001" }
        });
        return cart;
    }

    private void StockAvailable(int available)
    {
        _stockItems.Setup(r => r.FindAsync(It.IsAny<Expression<Func<StockItem, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockItem> { new() { ProductVariantId = _variantId, QuantityOnHand = available, QuantityReserved = 0 } });
    }

    private void AddressForUser()
    {
        _addresses.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Address
            {
                Id = Guid.NewGuid(),
                UserId = _userId,
                FullAddress = "10 Street",
                City = "Cairo",
                Governorate = "Cairo",
                PhoneNumber = "01000000000"
            });
    }

    private sealed class Slot<T>
    {
        public T? Value { get; set; }
    }

    private (Slot<Order> Order, Slot<OutboxMessage> OutboxMessage) CaptureCreated()
    {
        var orderSlot = new Slot<Order>();
        var outboxSlot = new Slot<OutboxMessage>();

        _orders.Setup(r => r.AddAsync(It.IsAny<Order>(), It.IsAny<CancellationToken>()))
            .Callback<Order, CancellationToken>((o, _) => orderSlot.Value = o)
            .Returns(Task.CompletedTask);

        _outbox.Setup(r => r.AddAsync(It.IsAny<OutboxMessage>(), It.IsAny<CancellationToken>()))
            .Callback<OutboxMessage, CancellationToken>((m, _) => outboxSlot.Value = m)
            .Returns(Task.CompletedTask);

        return (orderSlot, outboxSlot);
    }

    [Fact]
    public async Task Handle_should_create_pending_order_without_coupon()
    {
        var cart = CartWithOneItem(2, price: 100m);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);

        var addressId = Guid.NewGuid();
        AddressForUser();

        var (orderSlot, outboxSlot) = CaptureCreated();

        var unitOfWork = CreateUnitOfWork();
        var handler = CreateHandler(unitOfWork);
        var result = await handler.Handle(new CheckoutCommand(addressId, null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var order = orderSlot.Value;
        order.Should().NotBeNull();
        order!.CustomerId.Should().Be(_userId);
        order.Status.Should().Be(OrderStatus.Pending);
        order.SubTotal.Should().Be(200m);
        order.DiscountAmount.Should().Be(0m);
        order.TotalAmount.Should().Be(200m);
        order.CouponId.Should().BeNull();
        order.Items.Should().ContainSingle(i => i.Quantity == 2 && i.UnitPrice == 100m);
        order.ShippingAddressSnapshot.Should().Contain("Cairo");
        order.ShippingAddressSnapshot.Should().Contain("01000000000");

        outboxSlot.Value.Should().NotBeNull();
        outboxSlot.Value!.Type.Should().Be("OrderCreated");

        unitOfWork.Verify(u => u.CommitTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.Carts.Remove(cart), Times.Once);
    }

    [Fact]
    public async Task Handle_should_reserve_stock()
    {
        var cart = CartWithOneItem(3);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);

        AddressForUser();
        CaptureCreated();

        var handler = CreateHandler(CreateUnitOfWork());
        var result = await handler.Handle(new CheckoutCommand(Guid.NewGuid(), null), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        _stockItems.Verify(r => r.Update(It.Is<StockItem>(s => s.QuantityReserved == 3)), Times.Once);
    }

    [Fact]
    public async Task Handle_should_apply_percentage_coupon()
    {
        var cart = CartWithOneItem(2, price: 100m);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);
        AddressForUser();

        var coupon = new Coupon
        {
            Id = Guid.NewGuid(),
            Code = "SAVE10",
            DiscountType = DiscountType.Percentage,
            DiscountValue = 10,
            MaxUsageCount = 5,
            ExpiryDate = DateTime.UtcNow.AddDays(1),
            IsActive = true
        };

        _coupons.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Coupon> { coupon });

        var (orderSlot, _) = CaptureCreated();
        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(Guid.NewGuid(), "SAVE10"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var order = orderSlot.Value!;
        order.DiscountAmount.Should().Be(20m);
        order.TotalAmount.Should().Be(180m);
        order.CouponId.Should().Be(coupon.Id);
    }

    [Fact]
    public async Task Handle_should_apply_fixed_amount_coupon_capped_at_subtotal()
    {
        var cart = CartWithOneItem(1, price: 40m);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);
        AddressForUser();

        _coupons.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Coupon>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    Code = "FIXED50",
                    DiscountType = DiscountType.FixedAmount,
                    DiscountValue = 50m,
                    MaxUsageCount = 5,
                    ExpiryDate = DateTime.UtcNow.AddDays(1),
                    IsActive = true
                }
            });

        var (orderSlot, _) = CaptureCreated();
        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(Guid.NewGuid(), "FIXED50"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        var order = orderSlot.Value!;
        order.DiscountAmount.Should().Be(40m);
        order.TotalAmount.Should().Be(0m);
    }

    [Fact]
    public async Task Handle_should_fail_when_cart_is_empty()
    {
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(null as Cart);

        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(Guid.NewGuid(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Cart.Empty");
    }

    [Fact]
    public async Task Handle_should_fail_when_stock_is_insufficient_and_rollback()
    {
        var cart = CartWithOneItem(5);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(3);

        var unitOfWork = CreateUnitOfWork();
        var result = await CreateHandler(unitOfWork).Handle(new CheckoutCommand(Guid.NewGuid(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Stock.Insufficient");
        unitOfWork.Verify(u => u.RollbackTransactionAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_fail_when_coupon_not_found()
    {
        var cart = CartWithOneItem(1);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);

        _coupons.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Coupon>());

        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(Guid.NewGuid(), "MISSING"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Coupon.NotFound");
    }

    [Fact]
    public async Task Handle_should_fail_when_coupon_is_expired()
    {
        var cart = CartWithOneItem(1);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);

        _coupons.Setup(r => r.FindAsync(It.IsAny<Expression<Func<Coupon, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Coupon>
            {
                new() { Id = Guid.NewGuid(), Code = "OLD", DiscountType = DiscountType.Percentage, DiscountValue = 10, ExpiryDate = DateTime.UtcNow.AddMinutes(-1), IsActive = true }
            });

        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(Guid.NewGuid(), "OLD"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Coupon.Invalid");
    }

    [Fact]
    public async Task Handle_should_fail_when_address_not_found()
    {
        var cart = CartWithOneItem(1);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);

        _addresses.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Address);

        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(Guid.NewGuid(), null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Address.NotFound");
    }

    [Fact]
    public async Task Handle_should_fail_when_address_belongs_to_another_user()
    {
        var cart = CartWithOneItem(1);
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        StockAvailable(10);

        var addressId = Guid.NewGuid();
        _addresses.Setup(r => r.GetByIdAsync(addressId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Address { Id = addressId, UserId = Guid.NewGuid() });

        var result = await CreateHandler(CreateUnitOfWork()).Handle(new CheckoutCommand(addressId, null), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Address.NotFound");
    }
}