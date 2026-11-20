namespace TrailDriven.Result;

/// <summary>
/// Asynchronous versions of the helpers in <see cref="ResultExtensions"/>.
/// </summary>
/// <remarks>
/// <para>
/// Overloads that extend <see cref="Result{T}"/> or <see cref="Result"/> take asynchronous functions.
/// Overloads that extend <see cref="Task{TResult}"/> of a result await it first, so calls can be chained
/// directly off an asynchronous method, for example
/// <c>await GetUserAsync(id).MapAsync(u =&gt; u.Name).BindAsync(SaveAsync)</c>.
/// </para>
/// <para>
/// As with the synchronous helpers, failure cases pass through unchanged without calling the function,
/// and an uninitialized (<c>default</c>) result throws <see cref="InvalidOperationException"/>.
/// </para>
/// </remarks>
public static class ResultAsyncExtensions
{
    /// <summary>
    /// Transforms the success value with the asynchronous function <paramref name="map"/>. A failure case
    /// is returned unchanged and <paramref name="map"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The new success value type.</typeparam>
    /// <param name="result">The result to transform.</param>
    /// <param name="map">The asynchronous transform to apply to the success value.</param>
    /// <returns>A result holding the transformed value, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> MapAsync<T, U>(this Result<T> result, Func<T, Task<U>> map) => result switch
    {
        NotFound n => new Result<U>(n),
        Invalid i => new Result<U>(i),
        Forbidden f => new Result<U>(f),
        Conflict c => new Result<U>(c),
        Failure e => new Result<U>(e),
        T v => new Result<U>(await map(v).ConfigureAwait(false)),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Passes the success value to the asynchronous operation <paramref name="bind"/>, which can itself fail,
    /// and returns its result. A failure case is returned unchanged and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="result">The result to continue from.</param>
    /// <param name="bind">The next asynchronous operation, called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> BindAsync<T, U>(this Result<T> result, Func<T, Task<Result<U>>> bind) => result switch
    {
        NotFound n => new Result<U>(n),
        Invalid i => new Result<U>(i),
        Forbidden f => new Result<U>(f),
        Conflict c => new Result<U>(c),
        Failure e => new Result<U>(e),
        T v => await bind(v).ConfigureAwait(false),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then runs the function that matches the current case and returns its value.
    /// </summary>
    /// <typeparam name="T">The success value type.</typeparam>
    /// <typeparam name="TOut">The type returned by every branch.</typeparam>
    /// <param name="resultTask">The task producing the result to inspect.</param>
    /// <param name="ok">Called with the value when the result is a success.</param>
    /// <param name="notFound">Called when the result is <see cref="NotFound"/>.</param>
    /// <param name="invalid">Called when the result is <see cref="Invalid"/>.</param>
    /// <param name="forbidden">Called when the result is <see cref="Forbidden"/>.</param>
    /// <param name="conflict">Called when the result is <see cref="Conflict"/>.</param>
    /// <param name="failure">Called when the result is <see cref="Failure"/>.</param>
    /// <returns>The value returned by the branch that ran.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<TOut> MatchAsync<T, TOut>(
        this Task<Result<T>> resultTask,
        Func<T, TOut> ok,
        Func<NotFound, TOut> notFound,
        Func<Invalid, TOut> invalid,
        Func<Forbidden, TOut> forbidden,
        Func<Conflict, TOut> conflict,
        Func<Failure, TOut> failure) =>
        (await resultTask.ConfigureAwait(false)).Match(ok, notFound, invalid, forbidden, conflict, failure);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then transforms the success value with <paramref name="map"/>.
    /// A failure case is returned unchanged and <paramref name="map"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The new success value type.</typeparam>
    /// <param name="resultTask">The task producing the result to transform.</param>
    /// <param name="map">The transform to apply to the success value.</param>
    /// <returns>A result holding the transformed value, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, U> map) =>
        (await resultTask.ConfigureAwait(false)).Map(map);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then transforms the success value with the asynchronous function
    /// <paramref name="map"/>. A failure case is returned unchanged and <paramref name="map"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The new success value type.</typeparam>
    /// <param name="resultTask">The task producing the result to transform.</param>
    /// <param name="map">The asynchronous transform to apply to the success value.</param>
    /// <returns>A result holding the transformed value, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> MapAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<U>> map) =>
        await (await resultTask.ConfigureAwait(false)).MapAsync(map).ConfigureAwait(false);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then passes the success value to <paramref name="bind"/>, which can
    /// itself fail. A failure case is returned unchanged and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="bind">The next operation, called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> BindAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Result<U>> bind) =>
        (await resultTask.ConfigureAwait(false)).Bind(bind);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then passes the success value to the asynchronous operation
    /// <paramref name="bind"/>, which can itself fail. A failure case is returned unchanged and
    /// <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="T">The source success value type.</typeparam>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="bind">The next asynchronous operation, called with the success value.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> BindAsync<T, U>(this Task<Result<T>> resultTask, Func<T, Task<Result<U>>> bind) =>
        await (await resultTask.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then discards the success value, converting it to a non-generic
    /// <see cref="Result"/>. A success becomes <see cref="Success"/>, and a failure case is carried over unchanged.
    /// </summary>
    /// <typeparam name="T">The success value type being discarded.</typeparam>
    /// <param name="resultTask">The task producing the result to convert.</param>
    /// <returns>A <see cref="Result"/> with the same outcome and no value.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result> ToResultAsync<T>(this Task<Result<T>> resultTask) =>
        (await resultTask.ConfigureAwait(false)).ToResult();

    /// <summary>
    /// Calls the asynchronous operation <paramref name="bind"/> if <paramref name="result"/> is <see cref="Success"/>
    /// and returns its result. Otherwise returns <paramref name="result"/> unchanged.
    /// </summary>
    /// <param name="result">The result to continue from.</param>
    /// <param name="bind">The next asynchronous operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    public static async Task<Result> BindAsync(this Result result, Func<Task<Result>> bind) =>
        result is Success ? await bind().ConfigureAwait(false) : result;

    /// <summary>
    /// Calls the asynchronous operation <paramref name="bind"/> if <paramref name="result"/> is <see cref="Success"/>
    /// and returns its value-bearing result. A failure case is carried over unchanged and <paramref name="bind"/>
    /// is not called.
    /// </summary>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="result">The result to continue from.</param>
    /// <param name="bind">The next asynchronous operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> BindAsync<U>(this Result result, Func<Task<Result<U>>> bind) => result switch
    {
        Success => await bind().ConfigureAwait(false),
        NotFound n => new Result<U>(n),
        Invalid i => new Result<U>(i),
        Forbidden f => new Result<U>(f),
        Conflict c => new Result<U>(c),
        Failure e => new Result<U>(e),
        _ => throw new InvalidOperationException("Uninitialized result")
    };

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then runs the function that matches the current case and returns its value.
    /// </summary>
    /// <typeparam name="TOut">The type returned by every branch.</typeparam>
    /// <param name="resultTask">The task producing the result to inspect.</param>
    /// <param name="ok">Called when the result is <see cref="Success"/>.</param>
    /// <param name="notFound">Called when the result is <see cref="NotFound"/>.</param>
    /// <param name="invalid">Called when the result is <see cref="Invalid"/>.</param>
    /// <param name="forbidden">Called when the result is <see cref="Forbidden"/>.</param>
    /// <param name="conflict">Called when the result is <see cref="Conflict"/>.</param>
    /// <param name="failure">Called when the result is <see cref="Failure"/>.</param>
    /// <returns>The value returned by the branch that ran.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<TOut> MatchAsync<TOut>(
        this Task<Result> resultTask,
        Func<Success, TOut> ok,
        Func<NotFound, TOut> notFound,
        Func<Invalid, TOut> invalid,
        Func<Forbidden, TOut> forbidden,
        Func<Conflict, TOut> conflict,
        Func<Failure, TOut> failure) =>
        (await resultTask.ConfigureAwait(false)).Match(ok, notFound, invalid, forbidden, conflict, failure);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then calls <paramref name="bind"/> if the result is
    /// <see cref="Success"/>. Otherwise returns the result unchanged.
    /// </summary>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="bind">The next operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    public static async Task<Result> BindAsync(this Task<Result> resultTask, Func<Result> bind) =>
        (await resultTask.ConfigureAwait(false)).Bind(bind);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then calls the asynchronous operation <paramref name="bind"/> if the
    /// result is <see cref="Success"/>. Otherwise returns the result unchanged.
    /// </summary>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="bind">The next asynchronous operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    public static async Task<Result> BindAsync(this Task<Result> resultTask, Func<Task<Result>> bind) =>
        await (await resultTask.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then calls <paramref name="bind"/> if the result is
    /// <see cref="Success"/>. A failure case is carried over unchanged and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="bind">The next operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> BindAsync<U>(this Task<Result> resultTask, Func<Result<U>> bind) =>
        (await resultTask.ConfigureAwait(false)).Bind(bind);

    /// <summary>
    /// Awaits <paramref name="resultTask"/>, then calls the asynchronous operation <paramref name="bind"/> if the
    /// result is <see cref="Success"/>. A failure case is carried over unchanged and <paramref name="bind"/> is not called.
    /// </summary>
    /// <typeparam name="U">The success value type of the next operation.</typeparam>
    /// <param name="resultTask">The task producing the result to continue from.</param>
    /// <param name="bind">The next asynchronous operation.</param>
    /// <returns>The result of <paramref name="bind"/>, or the original failure case.</returns>
    /// <exception cref="InvalidOperationException">The result is uninitialized (<c>default</c>).</exception>
    public static async Task<Result<U>> BindAsync<U>(this Task<Result> resultTask, Func<Task<Result<U>>> bind) =>
        await (await resultTask.ConfigureAwait(false)).BindAsync(bind).ConfigureAwait(false);
}
