namespace TrailDriven.Result;

/// <summary>The case for a successful <see cref="Result"/> that carries no value.</summary>
public record Success;

/// <summary>The case for a requested resource that does not exist.</summary>
/// <param name="Message">An optional description of what was not found.</param>
public record NotFound(string? Message = null);

/// <summary>The case for input that failed validation.</summary>
/// <param name="Errors">The validation errors.</param>
public record Invalid(IReadOnlyList<ValidationError> Errors);

/// <summary>The case for a caller that is not allowed to perform the operation.</summary>
public record Forbidden;

/// <summary>The case for an operation that conflicts with the current state, such as a duplicate or a concurrency clash.</summary>
/// <param name="Message">An optional description of the conflict.</param>
public record Conflict(string? Message = null);

/// <summary>The case for an operation that failed unexpectedly.</summary>
/// <param name="Errors">Messages describing the failure.</param>
public record Failure(IReadOnlyList<string> Errors);

/// <summary>
/// The outcome of an operation that returns no value: either <see cref="Success"/> or one of the failure cases.
/// </summary>
/// <remarks>Converts implicitly from each case type. Use a <c>switch</c> to branch on the case.</remarks>
public union Result(Success, NotFound, Invalid, Forbidden, Conflict, Failure);

/// <summary>
/// The outcome of an operation that returns a <typeparamref name="T"/> on success, or one of the failure cases.
/// </summary>
/// <typeparam name="T">The success value type. Should not be one of the case types.</typeparam>
/// <remarks>Converts implicitly from <typeparamref name="T"/> and each case type. Use a <c>switch</c> to branch on the case.</remarks>
public union Result<T>(T, NotFound, Invalid, Forbidden, Conflict, Failure);
