namespace TrailDriven.Result;

/// <summary>The severity of a <see cref="ValidationError"/>.</summary>
public enum ValidationSeverity
{
    /// <summary>The input is invalid and the operation cannot proceed.</summary>
    Error,

    /// <summary>The input is questionable but the operation could proceed.</summary>
    Warning,

    /// <summary>Informational only.</summary>
    Info
}
