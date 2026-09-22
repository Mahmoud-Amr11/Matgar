using Matgar.Application.Abstractions.Caching;
using Matgar.Application.Abstractions.Identity;
using Matgar.Application.Abstractions.Persistence.Queries.ProductReview;
using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Features.ProductReview.Commands.CreateProductReview;

namespace Matgar.Application.Tests.Handlers;

public class CreateProductReviewCommandHandlerTests
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _productId = Guid.NewGuid();
    private readonly Guid _orderId = Guid.NewGuid();
    private readonly Mock<IProductRepository> _products = new();
    private readonly Mock<IProductReviewRepository> _reviews = new();
    private readonly Mock<IPurchaseVerificationQueries> _purchaseVerification = new();
    private readonly Mock<ICacheService> _cache = new();

    private CreateProductReviewCommandHandler CreateHandler()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Products).Returns(_products.Object);
        unitOfWork.Setup(u => u.ProductReviews).Returns(_reviews.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        var currentUser = new Mock<ICurrentUserService>();
        currentUser.Setup(u => u.UserId).Returns(_userId.ToString());

        return new CreateProductReviewCommandHandler(
            unitOfWork.Object,
            currentUser.Object,
            _purchaseVerification.Object,
            _cache.Object);
    }

    [Fact]
    public async Task Handle_should_create_review_when_purchase_is_verified()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = _productId });
        _purchaseVerification.Setup(q => q.FindReviewableDeliveredOrderIdAsync(_userId, _productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(_orderId);

        ProductReview? captured = null;
        _reviews.Setup(r => r.AddAsync(It.IsAny<ProductReview>(), It.IsAny<CancellationToken>()))
            .Callback<ProductReview, CancellationToken>((review, _) => captured = review)
            .Returns(Task.CompletedTask);

        var result = await CreateHandler().Handle(
            new CreateProductReviewCommand(_productId, 5, "Great product"),
            CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.UserId.Should().Be(_userId);
        captured.OrderId.Should().Be(_orderId);
        captured.Rating.Should().Be(5);
        captured.Comment.Should().Be("Great product");
        result.Value.Should().Be(captured.Id);
        _cache.Verify(c => c.RemoveByPrefixAsync($"GetProductReviews_{_productId}", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_fail_when_purchase_not_verified()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Product { Id = _productId });
        _purchaseVerification.Setup(q => q.FindReviewableDeliveredOrderIdAsync(_userId, _productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Guid?);

        var result = await CreateHandler().Handle(
            new CreateProductReviewCommand(_productId, 4, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Review.PurchaseNotVerified");
    }

    [Fact]
    public async Task Handle_should_fail_when_product_not_found()
    {
        _products.Setup(r => r.GetByIdAsync(_productId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(null as Product);

        var result = await CreateHandler().Handle(
            new CreateProductReviewCommand(_productId, 4, null),
            CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Product.NotFound");
    }
}