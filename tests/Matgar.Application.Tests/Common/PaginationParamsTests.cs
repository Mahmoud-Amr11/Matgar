using Matgar.Application.Common.Pagination;

namespace Matgar.Application.Tests.Common;

public class PaginationParamsTests
{
    [Theory]
    [InlineData(1, 20, 0)]
    [InlineData(2, 20, 20)]
    [InlineData(3, 10, 20)]
    [InlineData(0, 20, 0)]
    [InlineData(-2, 20, 0)]
    public void Offset_should_be_computed_from_normalized_page_and_size(
        int page, int pageSize, int expectedOffset)
    {
        var pagination = new PaginationParams { Page = page, PageSize = pageSize };

        pagination.Offset.Should().Be(expectedOffset);
    }

    [Fact]
    public void Negative_page_should_normalize_to_page_one()
    {
        var pagination = new PaginationParams { Page = -5 };

        pagination.NormalizedPage.Should().Be(1);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(101)]
    public void Out_of_range_page_size_should_fall_back_to_default(int pageSize)
    {
        var pagination = new PaginationParams { PageSize = pageSize };

        pagination.NormalizedPageSize.Should().Be(20);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(100)]
    public void In_range_page_size_should_be_kept(int pageSize)
    {
        var pagination = new PaginationParams { PageSize = pageSize };

        pagination.NormalizedPageSize.Should().Be(pageSize);
    }
}