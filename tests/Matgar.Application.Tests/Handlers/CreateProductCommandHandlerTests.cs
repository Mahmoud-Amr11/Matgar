using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Common.Caching;
using Matgar.Application.Common.Results;
using Matgar.Application.Features.Products.Commands.CreateProduct;
using System.Linq.Expressions;

namespace Matgar.Application.Tests.Handlers;

public class CreateProductCommandHandlerTests
{
    private readonly Guid _vendorId = Guid.NewGuid();
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICacheService> _cache = new();

    private static Mock<ICurrentUserService> CurrentUser(string? userId)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(userId);
        return currentUser;
    }

    private CreateProductCommandHandler CreateHandler(string? userId)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Categories).Returns(_categories.Object);
        unitOfWork.Setup(u => u.Products).Returns(_products.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new CreateProductCommandHandler(unitOfWork.Object, CurrentUser(userId).Object, _cache.Object);
    }

    private static Expression<Func<Category, bool>> AnyCategory() =>
        It.IsAny<Expression<Func<Category, bool>>>();

    [Fact]
    public async Task Handle_should_create_draft_product_for_vendor()
    {
        var categoryId = Guid.NewGuid();
        _categories.Setup(r => r.AnyAsync(AnyCategory(), It.IsAny<CancellationToken>())).ReturnsAsync(true);

        Product? captured = null;
        _products.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => captured = p)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler(_vendorId.ToString());
        var result = await handler.Handle(
            new CreateProductCommand("Wireless Mouse", "A nice mouse", categoryId),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.VendorId.Should().Be(_vendorId);
        captured.CategoryId.Should().Be(categoryId);
        captured.Status.Should().Be(ProductStatus.Draft);
        result.Value.Should().Be(captured.Id);
        _cache.Verify(c => c.RemoveByPrefixAsync("GetProducts", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_fail_when_vendor_identity_is_invalid()
    {
        var result = await CreateHandler("not-a-guid").Handle(
            new CreateProductCommand("Mouse", "desc", Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().errorType.Should().Be(ErrorType.Unauthorized);
    }

    [Fact]
    public async Task Handle_should_fail_when_category_does_not_exist()
    {
        _categories.Setup(r => r.AnyAsync(AnyCategory(), It.IsAny<CancellationToken>())).ReturnsAsync(false);

        var result = await CreateHandler(_vendorId.ToString()).Handle(
            new CreateProductCommand("Mouse", "desc", Guid.NewGuid()),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Category.NotFound");
        _products.Verify(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}