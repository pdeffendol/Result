namespace TrailDriven.Result.UnitTests;

public class GenericResultExtensionsTests
{
    [Theory]
    [InlineData("ok")]
    [InlineData("notFound")]
    [InlineData("invalid")]
    [InlineData("forbidden")]
    [InlineData("conflict")]
    [InlineData("failure")]
    public void Match_EachCase_CallsMatchingBranch(string caseName)
    {
        // Arrange
        var result = Create(caseName);

        // Act
        var branch = result.Match(
            ok: _ => "ok",
            notFound: _ => "notFound",
            invalid: _ => "invalid",
            forbidden: _ => "forbidden",
            conflict: _ => "conflict",
            failure: _ => "failure");

        // Assert
        branch.Should().Be(caseName);
    }

    [Fact]
    public void Match_Success_PassesValueToBranch()
    {
        // Arrange
        Result<int> result = 42;

        // Act
        var value = result.Match(ok: v => v, _ => 0, _ => 0, _ => 0, _ => 0, _ => 0);

        // Assert
        value.Should().Be(42);
    }

    [Fact]
    public void Map_Success_TransformsValue()
    {
        // Arrange
        Result<int> result = 21;

        // Act
        var mapped = result.Map(v => $"value {v * 2}");

        // Assert
        (mapped is string s && s == "value 42").Should().BeTrue();
    }

    [Theory]
    [InlineData("notFound")]
    [InlineData("invalid")]
    [InlineData("forbidden")]
    [InlineData("conflict")]
    [InlineData("failure")]
    public void Map_Failure_PreservesCaseWithoutCallingMap(string caseName)
    {
        // Arrange
        var result = Create(caseName);
        var called = false;

        // Act
        var mapped = result.Map(v => { called = true; return v.ToString(); });

        // Assert
        called.Should().BeFalse();
        CaseName(mapped).Should().Be(caseName);
    }

    [Fact]
    public void Map_Failure_PreservesCaseData()
    {
        // Arrange
        var result = Result<int>.Invalid(new ValidationError("Name", "Required"));

        // Act
        var mapped = result.Map(v => v.ToString());

        // Assert
        (mapped is Invalid i && i.Errors.Single() == new ValidationError("Name", "Required")).Should().BeTrue();
    }

    [Fact]
    public void Bind_Success_ReturnsResultOfBind()
    {
        // Arrange
        Result<int> result = 1;

        // Act
        var bound = result.Bind(_ => Result<string>.Conflict("taken"));

        // Assert
        (bound is Conflict c && c.Message == "taken").Should().BeTrue();
    }

    [Theory]
    [InlineData("notFound")]
    [InlineData("invalid")]
    [InlineData("forbidden")]
    [InlineData("conflict")]
    [InlineData("failure")]
    public void Bind_Failure_PreservesCaseWithoutCallingBind(string caseName)
    {
        // Arrange
        var result = Create(caseName);
        var called = false;

        // Act
        var bound = result.Bind(v => { called = true; return Result<string>.Ok(v.ToString()); });

        // Assert
        called.Should().BeFalse();
        CaseName(bound).Should().Be(caseName);
    }

    [Theory]
    [InlineData("ok", true)]
    [InlineData("notFound", false)]
    [InlineData("failure", false)]
    public void IsSuccess_ReturnsWhetherResultHoldsValue(string caseName, bool expected)
    {
        // Act
        var isSuccess = Create(caseName).IsSuccess();

        // Assert
        isSuccess.Should().Be(expected);
    }

    [Theory]
    [InlineData("ok", "success")]
    [InlineData("notFound", "notFound")]
    [InlineData("invalid", "invalid")]
    [InlineData("forbidden", "forbidden")]
    [InlineData("conflict", "conflict")]
    [InlineData("failure", "failure")]
    public void ToResult_MapsEachCase(string caseName, string expected)
    {
        // Act
        var result = Create(caseName).ToResult();

        // Assert
        NonGenericResultExtensionsTests.CaseName(result).Should().Be(expected);
    }

    [Fact]
    public void Helpers_ValueTypeIsSupertypeOfCase_TreatFailureAsFailure()
    {
        // Arrange
        Result<object> result = new NotFound("missing");

        // Act
        var isSuccess = result.IsSuccess();
        var branch = result.Match(_ => "ok", _ => "notFound", _ => "", _ => "", _ => "", _ => "");
        var mapped = result.Map(_ => "mapped");
        var bound = result.Bind(_ => Result<string>.Ok("bound"));
        var converted = result.ToResult();

        // Assert
        isSuccess.Should().BeFalse();
        branch.Should().Be("notFound");
        CaseName(mapped).Should().Be("notFound");
        CaseName(bound).Should().Be("notFound");
        NonGenericResultExtensionsTests.CaseName(converted).Should().Be("notFound");
    }

    [Fact]
    public void Helpers_DefaultResult_Throw()
    {
        // Arrange
        Result<int> result = default;

        // Act
        var actions = new Action[]
        {
            () => result.Match(_ => 0, _ => 0, _ => 0, _ => 0, _ => 0, _ => 0),
            () => result.Map(v => v),
            () => result.Bind(Result<int>.Ok),
            () => result.ToResult(),
        };

        // Assert
        foreach (var action in actions)
            action.Should().Throw<InvalidOperationException>();
    }

    internal static Result<int> Create(string caseName) => caseName switch
    {
        "ok" => Result<int>.Ok(1),
        "notFound" => Result<int>.NotFound("missing"),
        "invalid" => Result<int>.Invalid(new ValidationError("Field", "Bad")),
        "forbidden" => Result<int>.Forbidden(),
        "conflict" => Result<int>.Conflict("taken"),
        "failure" => Result<int>.Failure("boom"),
        _ => throw new ArgumentOutOfRangeException(nameof(caseName)),
    };

    internal static string CaseName<T>(Result<T> result) => result switch
    {
        NotFound => "notFound",
        Invalid => "invalid",
        Forbidden => "forbidden",
        Conflict => "conflict",
        Failure => "failure",
        T => "ok",
        _ => throw new InvalidOperationException("Uninitialized result"),
    };
}
