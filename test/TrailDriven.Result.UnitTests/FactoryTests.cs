namespace TrailDriven.Result.UnitTests;

public class FactoryTests
{
    [Fact]
    public void Factories_CarryCaseData()
    {
        // Arrange
        var error = new ValidationError("Name", "Required");

        // Act
        var notFound = Result<int>.NotFound("missing");
        var invalid = Result<int>.Invalid(error);
        var conflict = Result<int>.Conflict("taken");
        var failure = Result<int>.Failure("a", "b");

        // Assert
        (notFound is NotFound n && n.Message == "missing").Should().BeTrue();
        (invalid is Invalid i && i.Errors.SequenceEqual([error])).Should().BeTrue();
        (conflict is Conflict c && c.Message == "taken").Should().BeTrue();
        (failure is Failure f && f.Errors.SequenceEqual(["a", "b"])).Should().BeTrue();
    }

    [Fact]
    public void ImplicitConversion_FromValueAndCases_SelectsMatchingCase()
    {
        // Act
        Result<string> fromValue = "hello";
        Result<string> fromCase = new Forbidden();

        // Assert
        (fromValue is string s && s == "hello").Should().BeTrue();
        (fromCase is Forbidden).Should().BeTrue();
    }
}
