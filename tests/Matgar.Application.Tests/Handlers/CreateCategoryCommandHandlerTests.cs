using System.Linq.Expressions;
using Matgar.Application.Abstractions.Caching;
using Matgar.Application.Abstractions.Persistence.Repositories;
using Matgar.Application.Features.Category.Commands.CreateCategory;

namespace Matgar.Application.Tests.Handlers;

public class CreateCategoryCommandHandlerTests
{
    private readonly Mock<ICategoryRepository> _categories = new();
    private readonly Mock<ICacheService> _cache = new();

    private CreateCategoryCommandHandler CreateHandler()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Categories).Returns(_categories.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);

        return new CreateCategoryCommandHandler(unitOfWork.Object, _cache.Object);
    }

    [Fact]
    public async Task Handle_should_create_category_and_generate_slug()
    {
        _categories.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);

        Category? captured = null;
        _categories.Setup(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()))
            .Callback<Category, CancellationToken>((category, _) => captured = category)
            .Returns(Task.CompletedTask);

        var handler = CreateHandler();
        var result = await handler.Handle(new CreateCategoryCommand("My Test Category 123!"), CancellationToken.None);

        result.IsSuccess.Should().BeTrue();
        captured.Should().NotBeNull();
        captured!.Name.Should().Be("My Test Category 123!");
        captured.Slug.Should().Be("my-test-category-123");
        _cache.Verify(c => c.RemoveByPrefixAsync("categories:list", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_should_fail_when_name_already_exists()
    {
        _categories.Setup(r => r.AnyAsync(It.IsAny<Expression<Func<Category, bool>>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await CreateHandler().Handle(new CreateCategoryCommand("Clothes"), CancellationToken.None);

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().Code.Should().Be("Category.NameExists");
        _categories.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}