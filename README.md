# TrailDriven.Result

A small result library built on C# union types. It models the outcome of an
operation as either a success value or one of a fixed set of failure cases, and
the compiler checks that you handle every case.

## Requirements

- .NET 11 SDK or later (C# 15 union types)

## Installation

```shell
dotnet add package TrailDriven.Result
```

## Cases

| Case | Meaning | Data |
| --- | --- | --- |
| `T` / `Success` | The operation succeeded | The value (`Result<T>`), or nothing (`Result`) |
| `NotFound` | The requested resource does not exist | Optional `Message` |
| `Invalid` | The input failed validation | `Errors` (a list of `ValidationError`) |
| `Forbidden` | The caller is not allowed to perform the operation | None |
| `Conflict` | The operation conflicts with the current state | Optional `Message` |
| `Failure` | The operation failed unexpectedly | `Errors` (a list of messages) |

`Result<T>` is for operations that return a value. `Result` is for operations
that return nothing.

## Usage

### Returning a result

Results convert implicitly from the value and from each case type:

```csharp
using TrailDriven.Result;

public Result<User> GetUser(int id)
{
    if (id <= 0) return new Invalid([new ValidationError(nameof(id), "Id must be positive")]);
    if (!_users.TryGetValue(id, out var user)) return new NotFound($"User {id} not found");
    return user;
}
```

`Result<T>` also has static factories. They are most useful where the compiler
can't infer `T` from a bare case type, such as in a lambda passed to `Bind`:

```csharp
return Result<User>.Ok(user);
return Result<User>.NotFound($"User {id} not found");

var order = GetUser(id).Bind(u => u.IsActive ? PlaceOrder(u) : Result<Order>.Forbidden());
```

For a `Result` with no value, return `Result.Ok()` on success, or a case type
such as `new Conflict("Email already in use")` on failure.

### Handling a result

Use a `switch`. The compiler warns if a case is missing:

```csharp
var message = GetUser(id) switch
{
    User u => $"Found {u.Name}",
    NotFound n => n.Message ?? "Not found",
    Invalid i => string.Join("; ", i.Errors.Select(e => e.ErrorMessage)),
    Forbidden => "Access denied",
    Conflict c => c.Message ?? "Conflict",
    Failure f => string.Join("; ", f.Errors),
};
```

Or use `Match`, which takes one function per case:

```csharp
var message = DeleteUser(id).Match(
    ok: _ => "Deleted",
    notFound: n => n.Message ?? "Nothing to delete",
    invalid: _ => "Invalid request",
    forbidden: _ => "Access denied",
    conflict: _ => "Conflict",
    failure: _ => "Error");
```

> [!NOTE] When switching over a `Result<T>` where `T` is an unconstrained
> generic type parameter, the compiler reports warning CS8655 because `T` could
> be null. Add a `_ =>` arm to handle it.
>
> If `T` could be a supertype of a case type, such as `object` or an interface
> the case records implement, put the `T` arm after the case arms. Otherwise a
> failure case matches the `T` arm.

### Chaining

`Map` transforms a success value and `Bind` chains another operation that can
fail. Failure cases pass through unchanged, and the function is not called:

```csharp
Result<string> greeting = GetUser(id).Map(u => $"Hello, {u.Name}!");

Result<Order> order = GetUser(id).Bind(u => PlaceOrder(u, cart));
```

Other helpers:

- `IsSuccess()` returns whether the result is a success.
- `ToResult()` converts a `Result<T>` to a `Result`, discarding the value.

### Async

`MapAsync`, `BindAsync`, `MatchAsync` and `ToResultAsync` accept async
functions. They also extend `Task<Result<T>>` and `Task<Result>`, so you can
chain straight off an async method and mix sync and async steps:

```csharp
Result<OrderConfirmation> confirmation = await GetUserAsync(id)
    .BindAsync(u => PlaceOrderAsync(u, cart))
    .MapAsync(o => new OrderConfirmation(o.Id));
```

### Validation errors

`ValidationError` describes a single validation problem. Create one with a
constructor or an object initializer:

```csharp
new ValidationError("Email is required");
new ValidationError("Name", "Name is too long");
new ValidationError("Age", "Must be 18 or older", "AGE_MIN", ValidationSeverity.Warning);
new ValidationError { Identifier = "Zip", ErrorMessage = "Invalid format" };
```

`ErrorMessage` is required. `Severity` defaults to `ValidationSeverity.Error`.

## Building and testing

```shell
dotnet build
dotnet test
```

Tests use xUnit v3 on Microsoft.Testing.Platform. To collect code coverage:

```shell
dotnet test --coverage --coverage-output-format cobertura
```

## License

[MIT](https://github.com/pdeffendol/TrailDriven.Result/blob/main/LICENSE)
