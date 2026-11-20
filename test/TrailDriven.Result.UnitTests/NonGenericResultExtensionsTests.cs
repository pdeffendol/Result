namespace TrailDriven.Result.UnitTests;

public class NonGenericResultExtensionsTests
{
    [Theory]
    [InlineData("success")]
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
            ok: _ => "success",
            notFound: _ => "notFound",
            invalid: _ => "invalid",
            forbidden: _ => "forbidden",
            conflict: _ => "conflict",
            failure: _ => "failure");

        // Assert
        branch.Should().Be(caseName);
    }

    [Fact]
    public void Bind_Success_ReturnsResultOfBind()
    {
        // Act
        var bound = Result.Ok().Bind(() => new Forbidden());

        // Assert
        CaseName(bound).Should().Be("forbidden");
    }

    [Fact]
    public void BindToGeneric_Success_ReturnsResultOfBind()
    {
        // Act
        var bound = Result.Ok().Bind(() => Result<int>.Ok(7));

        // Assert
        (bound is int v && v == 7).Should().BeTrue();
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
        var bound = result.Bind(() => { called = true; return Result.Ok(); });
        var boundToGeneric = result.Bind(() => { called = true; return Result<int>.Ok(1); });

        // Assert
        called.Should().BeFalse();
        CaseName(bound).Should().Be(caseName);
        GenericResultExtensionsTests.CaseName(boundToGeneric).Should().Be(caseName);
    }

    [Theory]
    [InlineData("success", true)]
    [InlineData("notFound", false)]
    [InlineData("failure", false)]
    public void IsSuccess_ReturnsWhetherResultIsSuccess(string caseName, bool expected)
    {
        // Act
        var isSuccess = Create(caseName).IsSuccess();

        // Assert
        isSuccess.Should().Be(expected);
    }

    [Fact]
    public void Helpers_DefaultResult_Throw()
    {
        // Arrange
        Result result = default;

        // Act
        var actions = new Action[]
        {
            () => result.Match(_ => 0, _ => 0, _ => 0, _ => 0, _ => 0, _ => 0),
            () => result.Bind(() => Result<int>.Ok(1)),
        };

        // Assert
        foreach (var action in actions)
            action.Should().Throw<InvalidOperationException>();
    }

    private static Result Create(string caseName) => caseName switch
    {
        "success" => Result.Ok(),
        "notFound" => new NotFound("missing"),
        "invalid" => new Invalid([new ValidationError("Field", "Bad")]),
        "forbidden" => new Forbidden(),
        "conflict" => new Conflict("taken"),
        "failure" => new Failure(["boom"]),
        _ => throw new ArgumentOutOfRangeException(nameof(caseName)),
    };

    internal static string CaseName(Result result) => result switch
    {
        Success => "success",
        NotFound => "notFound",
        Invalid => "invalid",
        Forbidden => "forbidden",
        Conflict => "conflict",
        Failure => "failure",
    };
}
