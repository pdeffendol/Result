using System.Diagnostics.CodeAnalysis;

namespace TrailDriven.Result;

/// <summary>A single validation error, used by <see cref="Invalid"/>.</summary>
public record ValidationError
{
    /// <summary>
    /// Creates a validation error for use with an object initializer, which must set <see cref="ErrorMessage"/>.
    /// </summary>
    public ValidationError()
    {
    }

    /// <summary>Creates a validation error that is not tied to a specific field.</summary>
    /// <param name="errorMessage">A description of the problem.</param>
    [SetsRequiredMembers]
    public ValidationError(string errorMessage) => ErrorMessage = errorMessage;

    /// <summary>Creates a validation error for a specific field.</summary>
    /// <param name="identifier">The name of the field or input that failed validation.</param>
    /// <param name="errorMessage">A description of the problem.</param>
    [SetsRequiredMembers]
    public ValidationError(string identifier, string errorMessage)
    {
        Identifier = identifier;
        ErrorMessage = errorMessage;
    }

    /// <summary>Creates a validation error with an error code and severity.</summary>
    /// <param name="identifier">The name of the field or input that failed validation.</param>
    /// <param name="errorMessage">A description of the problem.</param>
    /// <param name="errorCode">A machine-readable code for the error.</param>
    /// <param name="severity">How serious the error is.</param>
    [SetsRequiredMembers]
    public ValidationError(string identifier, string errorMessage, string errorCode, ValidationSeverity severity)
    {
        Identifier = identifier;
        ErrorMessage = errorMessage;
        ErrorCode = errorCode;
        Severity = severity;
    }

    /// <summary>The name of the field or input that failed validation, or <see langword="null"/> if not field-specific.</summary>
    public string? Identifier { get; init; }

    /// <summary>A description of the problem.</summary>
    public required string ErrorMessage { get; init; }

    /// <summary>A machine-readable code for the error, if any.</summary>
    public string? ErrorCode { get; init; }

    /// <summary>How serious the error is. Defaults to <see cref="ValidationSeverity.Error"/>.</summary>
    public ValidationSeverity Severity { get; init; } = ValidationSeverity.Error;
}
