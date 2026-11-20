namespace TrailDriven.Result.UnitTests;

public class ResultAsyncExtensionsTests
{
    [Fact]
    public async Task MapAsync_Success_TransformsValue()
    {
        // Arrange
        Result<int> result = 21;

        // Act
        var mapped = await result.MapAsync(async v => { await Task.Yield(); return v * 2; });

        // Assert
        (mapped is int v && v == 42).Should().BeTrue();
    }

    [Fact]
    public async Task BindAsync_Success_ReturnsResultOfBind()
    {
        // Arrange
        Result<int> result = 1;

        // Act
        var bound = await result.BindAsync(_ => Task.FromResult(Result<string>.Conflict("taken")));

        // Assert
        (bound is Conflict c && c.Message == "taken").Should().BeTrue();
    }

    [Theory]
    [InlineData("notFound")]
    [InlineData("invalid")]
    [InlineData("forbidden")]
    [InlineData("conflict")]
    [InlineData("failure")]
    public async Task Helpers_Failure_PreserveCaseWithoutCallingFunction(string caseName)
    {
        // Arrange
        var result = GenericResultExtensionsTests.Create(caseName);
        var called = false;

        // Act
        var mapped = await result.MapAsync(v => { called = true; return Task.FromResult(v); });
        var bound = await result.BindAsync(v => { called = true; return Task.FromResult(Result<int>.Ok(v)); });
        var chained = await Task.FromResult(result)
            .MapAsync(v => { called = true; return v; })
            .BindAsync(v => { called = true; return Task.FromResult(Result<int>.Ok(v)); });

        // Assert
        called.Should().BeFalse();
        GenericResultExtensionsTests.CaseName(mapped).Should().Be(caseName);
        GenericResultExtensionsTests.CaseName(bound).Should().Be(caseName);
        GenericResultExtensionsTests.CaseName(chained).Should().Be(caseName);
    }

    [Fact]
    public async Task TaskChain_MixedSyncAndAsyncFunctions_ComposesInOrder()
    {
        // Arrange
        var resultTask = Task.FromResult(Result<int>.Ok(2));

        // Act
        var result = await resultTask
            .MapAsync(v => v + 1)
            .MapAsync(async v => { await Task.Yield(); return v * 10; })
            .BindAsync(v => Result<string>.Ok($"#{v}"))
            .BindAsync(async s => { await Task.Yield(); return Result<string>.Ok(s + "!"); });

        // Assert
        (result is string s && s == "#30!").Should().BeTrue();
    }

    [Theory]
    [InlineData("ok")]
    [InlineData("notFound")]
    [InlineData("failure")]
    public async Task MatchAsync_EachCase_CallsMatchingBranch(string caseName)
    {
        // Arrange
        var resultTask = Task.FromResult(GenericResultExtensionsTests.Create(caseName));

        // Act
        var branch = await resultTask.MatchAsync(
            ok: _ => "ok",
            notFound: _ => "notFound",
            invalid: _ => "invalid",
            forbidden: _ => "forbidden",
            conflict: _ => "conflict",
            failure: _ => "failure");

        // Assert
        branch.Should().Be(caseName);
    }

    [Theory]
    [InlineData("ok", "success")]
    [InlineData("conflict", "conflict")]
    public async Task ToResultAsync_MapsCase(string caseName, string expected)
    {
        // Act
        var result = await Task.FromResult(GenericResultExtensionsTests.Create(caseName)).ToResultAsync();

        // Assert
        NonGenericResultExtensionsTests.CaseName(result).Should().Be(expected);
    }

    [Fact]
    public async Task Helpers_ValueTypeIsSupertypeOfCase_TreatFailureAsFailure()
    {
        // Arrange
        Result<object> result = new NotFound("missing");

        // Act
        var mapped = await result.MapAsync(_ => Task.FromResult<object>("mapped"));
        var bound = await result.BindAsync(_ => Task.FromResult(Result<object>.Ok("bound")));

        // Assert
        GenericResultExtensionsTests.CaseName(mapped).Should().Be("notFound");
        GenericResultExtensionsTests.CaseName(bound).Should().Be("notFound");
    }

    [Fact]
    public async Task NonGenericBindAsync_Success_RunsEachOverload()
    {
        // Arrange
        var okTask = Task.FromResult(Result.Ok());

        // Act
        var syncBind = await okTask.BindAsync(() => new Forbidden());
        var asyncBind = await okTask.BindAsync(async () => { await Task.Yield(); return new Conflict("taken"); });
        var syncBindToGeneric = await okTask.BindAsync(() => Result<int>.Ok(1));
        var asyncBindToGeneric = await Result.Ok().BindAsync(() => Task.FromResult(Result<int>.Ok(2)));

        // Assert
        NonGenericResultExtensionsTests.CaseName(syncBind).Should().Be("forbidden");
        NonGenericResultExtensionsTests.CaseName(asyncBind).Should().Be("conflict");
        (syncBindToGeneric is int a && a == 1).Should().BeTrue();
        (asyncBindToGeneric is int b && b == 2).Should().BeTrue();
    }

    [Fact]
    public async Task NonGenericBindAsync_Failure_PreservesCaseWithoutCallingBind()
    {
        // Arrange
        var failedTask = Task.FromResult<Result>(new NotFound("missing"));
        var called = false;

        // Act
        var bound = await failedTask.BindAsync(() => { called = true; return Task.FromResult(Result.Ok()); });
        var boundToGeneric = await failedTask.BindAsync(() => { called = true; return Task.FromResult(Result<int>.Ok(1)); });

        // Assert
        called.Should().BeFalse();
        NonGenericResultExtensionsTests.CaseName(bound).Should().Be("notFound");
        GenericResultExtensionsTests.CaseName(boundToGeneric).Should().Be("notFound");
    }

    [Fact]
    public async Task NonGenericMatchAsync_Success_CallsOkBranch()
    {
        // Act
        var branch = await Task.FromResult(Result.Ok()).MatchAsync(
            ok: _ => "ok", _ => "", _ => "", _ => "", _ => "", _ => "");

        // Assert
        branch.Should().Be("ok");
    }

    [Fact]
    public async Task Helpers_DefaultResult_Throw()
    {
        // Arrange
        Result<int> result = default;

        // Act
        var act = () => result.MapAsync(v => Task.FromResult(v));

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
