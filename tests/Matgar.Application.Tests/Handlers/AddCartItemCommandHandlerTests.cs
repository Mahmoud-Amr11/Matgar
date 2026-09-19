using System.Linq.Expressions;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Features.Cart.Commands.AddCartItem;

namespace Matgar.Application.Tests.Handlers;

public class AddCartItemCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _variantId = Guid.NewGuid();
    private readonly Mock<IProductVariantRepository> _variants = new();
    private readonly Mock<ICartRepository> _carts = new();
    private readonly Mock<ICartItemRepository> _cartItems = new();
    private readonly Mock<IGenericRepository<StockItem>> _stockItems = new();

    private AddCartItemCommandHandler CreateHandler()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.ProductVariants).Returns(_variants.Object);
        unitOfWork.Setup(u => u.Carts).Returns(_carts.Object);
        unitOfWork.Setup(u => u.CartItems).Returns(_cartItems.Object);
        unitOfWork.Setup(u => u.StockItems).Returns(_stockItems.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(_userId.ToString());

        return new AddCartItemCommandHandler(unitOfWork.Object, currentUser.Object);
    }

    private ProductVariant ExistingVariant() => new()
    {
        Id = _variantId,
        Sku = "SKU-001",
        Price = 99.5m
    };

    private static Expression<Func<StockItem, bool>> AnyStockPredicate() =>
        It.IsAny<Expression<Func<StockItem, bool>>>();

    private void StockAvailable(int available)
    {
        _stockItems.Setup(r => r.FindAsync(AnyStockPredicate(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<StockItem>
            {
                new() { QuantityOnHand = available, QuantityReserved = 0 }
            });
    }

    [Fact]
    public async Task Handle_should_create_new_cart_and_item_with_price_snapshot()
    {
        _variants.Setup(r => r.GetByIdAsync(_variantId, It.IsAny<CancellationToken>())).ReturnsAsync(ExistingVariant());
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(null as Cart);
        StockAvailable(10);

        CartItem? captured = null;
        _cartItems.Setup(r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()))
            .Callback<CartItem, CancellationToken>((item, _) => captured = item)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(new AddCartItemCommand(_variantId, 2), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.ProductVariantId.Should().Be(_variantId);
        captured.Quantity.Should().Be(2);
        captured.PriceSnapshot.Should().Be(99.5m);
        result.Value.CartItemId.Should().Be(captured.Id);
        _carts.Verify(r => r.AddAsync(It.IsAny<Cart>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_merge_quantity_when_item_already_in_cart()
    {
        var cart = new Cart { Id = Guid.NewGuid(), UserId = _userId };
        var existing = new CartItem { Id = Guid.NewGuid(), CartId = cart.Id, ProductVariantId = _variantId, Quantity = 2 };

        _variants.Setup(r => r.GetByIdAsync(_variantId, It.IsAny<CancellationToken>())).ReturnsAsync(ExistingVariant());
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(cart);
        _cartItems.Setup(r => r.GetByCartAndVariantAsync(cart.Id, _variantId, It.IsAny<CancellationToken>())).ReturnsAsync(existing);
        StockAvailable(10);

        var result = await CreateHandler().Handle(new AddCartItemCommand(_variantId, 3), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        existing.Quantity.Should().Be(5);
        result.Value.Quantity.Should().Be(5);
        result.Value.CartId.Should().Be(cart.Id);
        _cartItems.Verify(r => r.Update(It.IsAny<CartItem>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_reject_quantity_exceeding_available_stock()
    {
        _variants.Setup(r => r.GetByIdAsync(_variantId, It.IsAny<CancellationToken>())).ReturnsAsync(ExistingVariant());
        _carts.Setup(r => r.GetByUserIdAsync(_userId, It.IsAny<CancellationToken>())).ReturnsAsync(null as Cart);
        StockAvailable(1);

        var result = await CreateHandler().Handle(new AddCartItemCommand(_variantId, 5), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("ProductVariant.InsufficientStock");
        _cartItems.Verify(r => r.AddAsync(It.IsAny<CartItem>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_should_fail_when_variant_not_found()
    {
        _variants.Setup(r => r.GetByIdAsync(_variantId, It.IsAny<CancellationToken>())).ReturnsAsync(null as ProductVariant);

        var result = await CreateHandler().Handle(new AddCartItemCommand(_variantId, 1), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("ProductVariant.NotFound");
    }
}