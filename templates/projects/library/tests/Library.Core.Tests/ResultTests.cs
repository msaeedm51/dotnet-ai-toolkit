using Library.Core;
using Xunit;

namespace Library.Core.Tests;

public class ResultTests
{
    [Fact]
    public void Success_HasNoError()
    {
        var result = Result.Success();

        Assert.True(result.IsSuccess);
        Assert.Null(result.Error);
    }

    [Fact]
    public void Failure_CarriesError()
    {
        var result = Result.Failure("Something went wrong.");

        Assert.False(result.IsSuccess);
        Assert.Equal("Something went wrong.", result.Error);
    }

    [Fact]
    public void GenericSuccess_ExposesValue()
    {
        var result = Result<int>.Success(42);

        Assert.True(result.IsSuccess);
        Assert.Equal(42, result.Value);
    }

    [Fact]
    public void GenericFailure_AccessingValue_Throws()
    {
        var result = Result<int>.Failure("Not found.");

        Assert.Throws<InvalidOperationException>(() => result.Value);
    }
}
