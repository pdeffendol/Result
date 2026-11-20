namespace TrailDriven.Result.UnitTests;

public class ValidationErrorTests
{
    [Fact]
    public void Constructor_WithoutSeverity_DefaultsToError()
    {
        // Act
        var error = new ValidationError("Name", "Required");

        // Assert
        error.Severity.Should().Be(ValidationSeverity.Error);
        error.ErrorCode.Should().BeNull();
    }

    [Fact]
    public void Constructor_MessageOnly_HasNoIdentifier()
    {
        // Act
        var error = new ValidationError("Required");

        // Assert
        error.Identifier.Should().BeNull();
        error.ErrorMessage.Should().Be("Required");
    }

    [Fact]
    public void Equals_SameValues_ReturnsTrue()
    {
        // Arrange
        var a = new ValidationError("Age", "Too young", "AGE_MIN", ValidationSeverity.Warning);
        var b = new ValidationError { Identifier = "Age", ErrorMessage = "Too young", ErrorCode = "AGE_MIN", Severity = ValidationSeverity.Warning };

        // Assert
        a.Should().Be(b);
    }
}
