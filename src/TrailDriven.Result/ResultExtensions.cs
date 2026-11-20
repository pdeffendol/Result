namespace TrailDriven.Result;

/// <summary>
/// Factory members and functional helpers for creating, consuming and composing
/// <see cref="Result"/> and <see cref="Result{T}"/> values.
/// </summary>
/// <remarks>
/// The helpers that inspect a result throw <see cref="InvalidOperationException"/> if it is <c>default</c>,
/// meaning it was never assigned a case.
/// </remarks>
public static class ResultExtensions
{
    /// <summary>
    /// Runs the function that matches the current case of <paramref name="result"/> and returns its value.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TOut">The type returned by every branch.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="ok">Called with the value when the result is a success.</param>
    /// <param name="notFound">Called when the result is <see cref="NotFound"/>.</param>
    /// <param name="invalid">Called when the result is <see cref="Invalid"/>.</param>
    /// <param name="forbidden">Called when the result is <see cref="Forbidden"/>.</param>
    /// <param name="conflict">Called when the result is <see cref="Conflict"/>.</param>
    /// <param name="failure">Called when the result is <see cref="Failure"/>.</param>
    /// <returns>The value returned by the branch that ran.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static TOut Match<T, TOut>(
        this Result<T> result,
        Func<T, TOut> ok,
        Func<NotFound, TOut> notFound,
        Func<Invalid, TOut> invalid,
        Func<Forbidden, TOut> forbidden,
        Func<Conflict, TOut> conflict,
        Func<Failure, TOut> failure) => result switch
        {
            NotFound n => notFound(n),
            Invalid i => invalid(i),
            Forbidden f => forbidden(f),
            Conflict c => conflict(c),
            Failure e => failure(e),
            T v => ok(v),
            _ => throw new InvalidOperationException("Uninitialized result")
        };

    /// <summary>
    /// Transforms the success value with <paramref name="map"/>. A failure case is returned unchanged
    /// and <paramref name="map"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The new success value type.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="map">The transform to apply to the success value.</param>
    /// <returns>A result holding the transformed value, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static Result<U> Map<T, U>(this Result<T> result, Func<T, U> map) => result switch
    {
        NotFound n => new Result<U>(n),
        Invalid i => new Result<U>(i),
        Forbidden f => new Result<U>(f),
        Conflict c => new Result<U>(c),
        Failure e => new Result<U>(e),
        T v => new Result<U>(map(v)),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Passes the success value to <paramref name="bind"/>, which can itself fail, and returns its result.
    /// A failure case is returned unchanged and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="result">The result to continue from.</param>
    /// <param name="bind">The next operation, called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static Result<U> Bind<T, U>(this Result<T> result, Func<T, Result<U>> bind) => result switch
    {
        NotFound n => new Result<U>(n),
        Invalid i => new Result<U>(i),
        Forbidden f => new Result<U>(f),
        Conflict c => new Result<U>(c),
        Failure e => new Result<U>(e),
        T v => bind(v),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Returns whether <paramref name="result"/> holds a success value.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <param name="result">The result to check.</param>
    /// <returns><see langword="true"/> if the result holds a <typeparamref name="T"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsSuccess<T>(this Result<T> result) => result switch
    {
        NotFound _ or Invalid _ or Forbidden _ or Conflict _ or Failure _ => false,
        T => true,
        _ => false
    };

    /// <summary>
    /// Runs the function that matches the current case of <paramref name="result"/> and returns its value.
    /// </summary>
    /// <typeparam name="TOut">The type returned by every branch.</typeparam>
    /// <param name="result">The result to inspect.</param>
    /// <param name="ok">Called when the result is <see cref="Success"/>.</param>
    /// <param name="notFound">Called when the result is <see cref="NotFound"/>.</param>
    /// <param name="invalid">Called when the result is <see cref="Invalid"/>.</param>
    /// <param name="forbidden">Called when the result is <see cref="Forbidden"/>.</param>
    /// <param name="conflict">Called when the result is <see cref="Conflict"/>.</param>
    /// <param name="failure">Called when the result is <see cref="Failure"/>.</param>
    /// <returns>The value returned by the branch that ran.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static TOut Match<TOut>(
        this Result result,
        Func<Success, TOut> ok,
        Func<NotFound, TOut> notFound,
        Func<Invalid, TOut> invalid,
        Func<Forbidden, TOut> forbidden,
        Func<Conflict, TOut> conflict,
        Func<Failure, TOut> failure) => result switch
        {
            Success s => ok(s),
            NotFound n => notFound(n),
            Invalid i => invalid(i),
            Forbidden f => forbidden(f),
            Conflict c => conflict(c),
            Failure e => failure(e),
            _ => throw new InvalidOperationException("Uninitialized result")
        };

    /// <summary>
    /// Calls <paramref name="bind"/> if <paramref name="result"/> is <see cref="Success"/> and returns
    /// its value-bearing result. A failure case is carried over unchanged and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="result">The result to continue from.</param>
    /// <param name="bind">The next operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static Result<U> Bind<U>(this Result result, Func<Result<U>> bind) => result switch
    {
        Success => bind(),
        NotFound n => new Result<U>(n),
        Invalid i => new Result<U>(i),
        Forbidden f => new Result<U>(f),
        Conflict c => new Result<U>(c),
        Failure e => new Result<U>(e),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Calls <paramref name="bind"/> if <paramref name="result"/> is <see cref="Success"/> and returns
    /// its result. Otherwise returns <paramref name="result"/> unchanged.
    /// </summary>
    /// <param name="result">The result to continue from.</param>
    /// <param name="bind">The next operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    public static Result Bind(this Result result, Func<Result> bind) => result is Success ? bind() : result;

    /// <summary>
    /// Discards the success value, converting <paramref name="result"/> to a non-generic <see cref="Result"/>.
    /// A success becomes <see cref="Success"/>, and a failure case is carried over unchanged.
    /// </summary>
    /// <typeparam name="T">The success value type being discarded.</typeparam>
    /// <param name="result">The result to convert.</param>
    /// <returns>A <see cref="Result"/> with the same outcome and no value.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static Result ToResult<T>(this Result<T> result) => result switch
    {
        NotFound n => new Result(n),
        Invalid i => new Result(i),
        Forbidden f => new Result(f),
        Conflict c => new Result(c),
        Failure e => new Result(e),
        T => new Result(new Success()),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Returns whether <paramref name="result"/> is <see cref="Success"/>.
    /// </summary>
    /// <param name="result">The result to check.</param>
    /// <returns><see langword="true"/> if the result is <see cref="Success"/>; otherwise <see langword="false"/>.</returns>
    public static bool IsSuccess(this Result result) => result is Success;

    /// <summary>Static factory members for <see cref="Result"/>.</summary>
    /// <remarks>
    /// <see cref="Result"/> converts implicitly from each case type, so failures are returned directly,
    /// for example <c>return new NotFound();</c>.
    /// </remarks>
    extension(Result)
    {
        /// <summary>Creates a successful <see cref="Result"/> with no value.</summary>
        /// <returns>A result in the <see cref="Success"/> case.</returns>
        public static Result Ok() => new(new Success());
    }

    /// <summary>Static factory members for <see cref="Result{T}"/>.</summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <remarks>
    /// <see cref="Result{T}"/> also converts implicitly from <typeparamref name="T"/> and each case type,
    /// so <c>return value;</c> or <c>return new NotFound();</c> works without these factories. The factories
    /// are useful where the compiler cannot infer <typeparamref name="T"/>, such as in a lambda passed to
    /// <see cref="Bind{T, U}(Result{T}, Func{T, Result{U}})"/>.
    /// </remarks>
    extension<T>(Result<T>)
    {
        /// <summary>Creates a successful <see cref="Result{T}"/> holding <paramref name="value"/>.</summary>
        /// <param name="value">The success value.</param>
        /// <returns>A result holding <paramref name="value"/>.</returns>
        public static Result<T> Ok(T value) => new(value);

        /// <summary>Creates a <see cref="Result{T}"/> indicating the requested resource was not found.</summary>
        /// <param name="message">An optional description of what was not found.</param>
        /// <returns>A result in the <see cref="global::TrailDriven.Result.NotFound"/> case.</returns>
        public static Result<T> NotFound(string? message = null) => new(new NotFound(message));

        /// <summary>Creates a <see cref="Result{T}"/> indicating the input failed validation.</summary>
        /// <param name="errors">The validation errors.</param>
        /// <returns>A result in the <see cref="global::TrailDriven.Result.Invalid"/> case.</returns>
        public static Result<T> Invalid(params ValidationError[] errors) => new(new Invalid(errors));

        /// <summary>Creates a <see cref="Result{T}"/> indicating the caller is not allowed to perform the operation.</summary>
        /// <returns>A result in the <see cref="global::TrailDriven.Result.Forbidden"/> case.</returns>
        public static Result<T> Forbidden() => new(new Forbidden());

        /// <summary>Creates a <see cref="Result{T}"/> indicating the operation conflicts with the current state.</summary>
        /// <param name="message">An optional description of the conflict.</param>
        /// <returns>A result in the <see cref="global::TrailDriven.Result.Conflict"/> case.</returns>
        public static Result<T> Conflict(string? message = null) => new(new Conflict(message));

        /// <summary>Creates a <see cref="Result{T}"/> indicating the operation failed unexpectedly.</summary>
        /// <param name="errors">Messages describing the failure.</param>
        /// <returns>A result in the <see cref="global::TrailDriven.Result.Failure"/> case.</returns>
        public static Result<T> Failure(params string[] errors) => new(new Failure(errors));
    }
}
