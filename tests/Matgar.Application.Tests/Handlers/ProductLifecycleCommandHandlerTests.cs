using Matgar.Application.Abstractions.Caching;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Features.Products.Commands.ApproveProduct;
using Matgar.Application.Features.Products.Commands.SubmitProductForReview;
using Matgar.Application.Features.Products.Commands.SuspendProduct;

namespace Matgar.Application.Tests.Handlers;

public class ProductLifecycleCommandHandlerTests
{
    private readonly Guid _vendorId = Guid.NewGuid();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<ICacheService> _cache = new();

    private Mock<ICurrentUserService> CurrentUser(string? userId = null)
    {
        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(userId ?? _vendorId.ToString());
        return currentUser;
    }

    private Mock<IUnitOfWork> UnitOfWork() => new();

    private Mock<IUnitOfWork> UnitOfWorkWithProducts()
    {
        var unitOfWork = UnitOfWork();
        unitOfWork.Setup(u => u.Products).Returns(_products.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
        return unitOfWork;
    }

    private Product ExistingProduct(ProductStatus status) => new()
    {
        Id = Guid.NewGuid(),
        VendorId = _vendorId,
        Status = status
    };

    // ---- SubmitProductForReview ----

    [Fact]
    public async Task Submit_should_move_draft_to_pending_review()
    {
        var product = ExistingProduct(ProductStatus.Draft);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var handler = new SubmitProductForReviewCommandHandler(UnitOfWorkWithProducts().Object, CurrentUser().Object, _cache.Object);
        var result = await handler.Handle(new SubmitProductForReviewCommand(product.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.PendingReview);
    }

    [Fact]
    public async Task Submit_should_fail_when_product_is_not_draft()
    {
        var product = ExistingProduct(ProductStatus.Active);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new SubmitProductForReviewCommandHandler(UnitOfWorkWithProducts().Object, CurrentUser().Object, _cache.Object)
            .Handle(new SubmitProductForReviewCommand(product.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.InvalidStatusTransition");
    }

    [Fact]
    public async Task Submit_should_fail_when_vendor_owns_another_product()
    {
        var product = ExistingProduct(ProductStatus.Draft);
        product.VendorId = Guid.NewGuid();
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new SubmitProductForReviewCommandHandler(UnitOfWorkWithProducts().Object, CurrentUser().Object, _cache.Object)
            .Handle(new SubmitProductForReviewCommand(product.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.NotFound");
    }

    // ---- ApproveProduct ----

    [Fact]
    public async Task Approve_should_move_pending_review_to_active()
    {
        var product = ExistingProduct(ProductStatus.PendingReview);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new ApproveProductCommandHandler(UnitOfWorkWithProducts().Object, _cache.Object)
            .Handle(new ApproveProductCommand(product.Id), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Active);
        _cache.Verify(c => c.RemoveByPrefixAsync("GetProducts", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Approve_should_fail_when_product_is_not_pending_review()
    {
        var product = ExistingProduct(ProductStatus.Draft);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new ApproveProductCommandHandler(UnitOfWorkWithProducts().Object, _cache.Object)
            .Handle(new ApproveProductCommand(product.Id), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.InvalidStatusTransition");
    }

    [Fact]
    public async Task Approve_should_fail_when_product_missing()
    {
        _products.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync(null as Product);

        var result = await new ApproveProductCommandHandler(UnitOfWorkWithProducts().Object, _cache.Object)
            .Handle(new ApproveProductCommand(Guid.NewGuid()), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.NotFound");
    }

    // ---- SuspendProduct ----

    [Fact]
    public async Task Suspend_should_set_product_to_suspended()
    {
        var product = ExistingProduct(ProductStatus.Active);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new SuspendProductCommandHandler(UnitOfWorkWithProducts().Object, _cache.Object)
            .Handle(new SuspendProductCommand(product.Id, "policy violation"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        product.Status.Should().Be(ProductStatus.Suspended);
    }

    [Fact]
    public async Task Suspend_should_fail_when_product_already_suspended()
    {
        var product = ExistingProduct(ProductStatus.Suspended);
        _products.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        var result = await new SuspendProductCommandHandler(UnitOfWorkWithProducts().Object, _cache.Object)
            .Handle(new SuspendProductCommand(product.Id, "again"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.AlreadySuspended");
    }
}