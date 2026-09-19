using Matgar.Application.Common.Results;

namespace Matgar.Application.Tests.Common;

public class ResultTests
{
    [Fact]
    public void Success_should_be_successful_with_no_errors()
    {
        var result = Result.Success;

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void Failure_with_single_error_should_expose_that_error()
    {
        var result = Result.Failure(Error.Conflict("Category.NameExists", "exists"));

        result.IsFailure.Should().BeTrue();
        result.IsSuccess.Should().BeFalse();
        result.Errors.Should().ContainSingle(e => e.Code == "Category.NameExists");
    }

    [Fact]
    public void Failure_with_list_of_errors_should_expose_all()
    {
        var errors = new List<Error> { Error.Validation("A", "a"), Error.Validation("B", "b") };

        var result = Result.Failure(errors);

        result.Errors.Should().HaveCount(2);
    }

    [Fact]
    public void Implicit_conversion_from_error_should_produce_failure()
    {
        Result result = Error.NotFound();

        result.IsFailure.Should().BeTrue();
        result.Errors.Single().errorType.Should().Be(ErrorType.NotFound);
    }

    [Fact]
    public void ResultOfT_Success_should_expose_value()
    {
        var value = Guid.NewGuid();

        var result = Result<Guid>.Success(value);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void ResultOfT_Failure_should_have_default_value()
    {
        var result = Result<int>.Failure(Error.Validation("V", "bad"));

        result.IsFailure.Should().BeTrue();
        result.Value.Should().Be(0);
    }

    [Fact]
    public void ResultOfT_implicit_conversion_from_value_should_be_success()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }
}