using System.Linq.Expressions;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Caching;
using Matgar.Application.Features.ProductVariant.Commands.CreateProductVariant;

namespace Matgar.Application.Tests.Handlers;

public class CreateProductVariantCommandHandlerTests
{
    private readonly Guid _vendorId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IProductVariantRepository> _variants = new();
    private readonly Mock<ICacheService> _cache = new();

    private CreateProductVariantCommandHandler CreateHandler(string? userId = null)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Products).Returns(_products.Object);
        unitOfWork.Setup(u => u.ProductVariants).Returns(_variants.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(userId ?? _vendorId.ToString());

        return new CreateProductVariantCommandHandler(unitOfWork.Object, currentUser.Object, _cache.Object);
    }

    private static Expression<Func<ProductVariant, bool>> AnyVariant() =>
        It.IsAny<Expression<Func<ProductVariant, bool>>>();

    private Product ExistingProduct(Guid? vendorId = null) => new()
    {
        Id = _productId,
        VendorId = vendorId ?? _vendorId,
        Status = ProductStatus.Draft
    };

    [Fact]
    public async Task Handle_should_create_variant_with_stock_item()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>())).ReturnsAsync(ExistingProduct());
        _variants.Setup(r => r.AnyAsync(AnyVariant(), It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _variants.Setup(r => r.FindAsync(AnyVariant(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ProductVariant>());

        ProductVariant? captured = null;
        _variants.Setup(r => r.AddAsync(It.IsAny<ProductVariant>(), It.IsAny<CancellationToken>()))
            .Callback<ProductVariant, CancellationToken>((v, _) => captured = v)
            .Returns(Task.CompletedTask);

        var command = new CreateProductVariantCommand(
            _productId,
            "SKU-001",
            299.99m,
            "image.png",
            """{"Color":"Black","Size":"M"}""",
            15);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Sku.Should().Be("SKU-001");
        captured.Price.Should().Be(299.99m);
        captured.StockItem.Should().NotBeNull();
        captured.StockItem!.QuantityOnHand.Should().Be(15);
        captured.StockItem.QuantityReserved.Should().Be(0);
        _cache.Verify(c => c.RemoveAsync($"GetProductVariants_{_productId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_fail_when_sku_already_exists()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>())).ReturnsAsync(ExistingProduct());
        _variants.Setup(r => r.AnyAsync(AnyVariant(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateHandler().Handle(
            new CreateProductVariantCommand(_productId, "SKU-001", 10m, null, "{}", 1),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Variant.SkuExists");
    }

    [Fact]
    public async Task Handle_should_fail_when_same_attribute_combination_exists()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>())).ReturnsAsync(ExistingProduct());
        _variants.Setup(r => r.AnyAsync(AnyVariant(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var existing = new ProductVariant { Id = Guid.NewGuid(), ProductId = _productId, AttributesJson = """{"Color":"Red","Size":"M"}""" };
        _variants.Setup(r => r.FindAsync(AnyVariant(), It.IsAny<CancellationToken>())).ReturnsAsync(new List<ProductVariant> { existing });

        var command = new CreateProductVariantCommand(
            _productId, "SKU-002", 10m, null, """{"Size":"M","Color":"Red"}""", 1);

        var result = await CreateHandler().Handle(command, CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Variant.CombinationExists");
    }

    [Fact]
    public async Task Handle_should_fail_when_product_belongs_to_another_vendor()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(ExistingProduct(Guid.NewGuid()));

        var result = await CreateHandler().Handle(
            new CreateProductVariantCommand(_productId, "SKU-003", 10m, null, "{}", 1),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.NotFound");
    }

    [Fact]
    public async Task Handle_should_fail_when_product_is_not_found()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>())).ReturnsAsync(null as Product);

        var result = await CreateHandler().Handle(
            new CreateProductVariantCommand(_productId, "SKU-004", 10m, null, "{}", 1),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.NotFound");
    }
}