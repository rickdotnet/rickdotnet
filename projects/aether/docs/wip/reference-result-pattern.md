# Result Pattern Reference

## Overview

The Result pattern in RickDotNet.Base provides a pragmatic approach to error handling in .NET applications. It's designed to be used where it makes sense - not as a wholesale replacement for exceptions, but as a tool to avoid using exceptions for control flow and to enable functional composition of operations.

## Philosophy

The Result pattern implementation follows these principles:

1. **Pragmatic Usage**: Use Result<T> where it adds value, not everywhere
2. **Control Flow**: Avoid throwing exceptions for expected failures
3. **Chainability**: Enable functional composition through extension methods
4. **Clarity**: Distinguish between errors (expected) and failures (unexpected exceptions)
5. **Async-First**: Full support for async operations throughout

## Core Types

### Result<T>

A discriminated union representing the outcome of an operation with three possible states:

```csharp
public abstract record Result<T>
{
    public sealed record Success(T Value) : Result<T>;
    public sealed record Error(string ErrorMessage) : Result<T>;
    public sealed record Failure(Exception Exception) : Result<T>;
}
```

#### States Explained

- **Success**: The operation completed successfully with a value
- **Error**: The operation failed with an expected error (business logic failure)
- **Failure**: The operation failed with an unexpected exception

### Unit Type

For operations that don't return a value (void-like operations):

```csharp
public struct Unit
{
    public static readonly Unit Default = new();
}
```

## Creating Results

### Factory Methods

```csharp
// Success results
Result<int> success = Result.Success(42);
Result<Unit> voidSuccess = Result.Success();

// Error results (expected failures)
Result<int> error = Result.Error<int>("User not found");
Result<Unit> voidError = Result.Error("Invalid operation");

// Failure results (exceptions)
Result<int> failure = Result.Failure<int>(new InvalidOperationException());
```

### Implicit Conversions

```csharp
// From value to Success
Result<int> result = 42;  // Implicit Success

// From exception to Failure
Result<int> failed = new InvalidOperationException("Boom!");

// Boolean conversion (true if Success)
if (result) {
    // Handle success
}
```

## Try Methods

Wrap operations that might throw exceptions:

### Synchronous Operations

```csharp
// Basic try
Result<int> result = Result.Try(() => {
    return int.Parse("123");
});

// Try with custom error message
Result<int> result = Result.Try(
    () => int.Parse(userInput),
    "Invalid number format"
);

// Try for void operations
Result<Unit> result = Result.Try(() => {
    File.Delete(path);
});
```

### Asynchronous Operations

```csharp
// Async try
Result<string> result = await Result.TryAsync(async () => {
    return await httpClient.GetStringAsync(url);
});

// Async try with custom error
Result<Data> result = await Result.TryAsync(
    async () => await LoadDataAsync(),
    "Failed to load data"
);

// Try async Result-returning functions
Result<User> result = await Result.TryAsync(async () => {
    return await GetUserAsync(id); // Returns Result<User>
});
```

## Extension Methods

### Select (Map)

Transform the success value:

```csharp
Result<int> number = Result.Success(5);
Result<string> text = number.Select(n => n.ToString());

// Async select
Result<string> data = await result.SelectAsync(async value => {
    return await ProcessAsync(value);
});
```

### Bind (FlatMap)

Chain operations that return Results:

```csharp
Result<User> GetUser(int id) => // ...
Result<Profile> GetProfile(User user) => // ...

Result<Profile> profile = GetUser(123)
    .Bind(user => GetProfile(user));

// Async bind
Result<Data> result = await LoadDataAsync()
    .BindAsync(async data => await ValidateAsync(data))
    .BindAsync(async valid => await SaveAsync(valid));
```

### Or (Fallback)

Provide fallback values for errors:

```csharp
Result<Config> config = LoadConfig()
    .Or(error => Config.Default);

// Async or
Result<Data> data = await LoadPrimaryAsync()
    .OrAsync(error => LoadFallbackData());
```

### Value Extraction

```csharp
// Get value or default
int value = result.ValueOrDefault(0);
string text = result.ValueOrDefault("default");

// Get value or null
int? maybeValue = result.ValueOrDefault();

// Async value extraction
int value = await resultTask.ValueOrDefaultAsync(0);
```

## Handling Results

### Pattern Matching

```csharp
string message = result switch {
    Result<int>.Success success => $"Got {success.Value}",
    Result<int>.Error error => $"Error: {error.ErrorMessage}",
    Result<int>.Failure failure => $"Exception: {failure.Exception.Message}",
    _ => "Unknown"
};
```

### Callback Methods

```csharp
// Handle success only
result.OnSuccess(value => {
    Console.WriteLine($"Success: {value}");
});

// Handle errors (both Error and Failure)
result.OnError(errorMessage => {
    Logger.LogError(errorMessage);
});

// Handle failures specifically
result.OnFailure(exception => {
    Logger.LogException(exception);
});

// Resolve with different handlers
result.Resolve(
    onSuccess: value => ProcessValue(value),
    onError: error => HandleError(error)
);

// Full resolve with separate failure handling
result.Resolve(
    onSuccess: value => ProcessValue(value),
    onError: error => HandleError(error),
    onFailure: ex => HandleException(ex)
);
```

### Async Handlers

```csharp
await result.OnSuccessAsync(async value => {
    await SaveToDatabase(value);
});

await result.ResolveAsync(
    onSuccess: async value => await ProcessAsync(value),
    onError: async error => await LogErrorAsync(error)
);
```

## Real-World Examples from Aether

### Storage Operations

```csharp
public interface IStore
{
    ValueTask<Result<T>> Get<T>(string id, CancellationToken token = default);
    ValueTask<Result<T>> Upsert<T>(string id, T data, CancellationToken token = default);
    ValueTask<Result<AetherData>> Delete(string id, CancellationToken token = default);
}

// Usage
var result = await store.Get<User>("user-123");
result.Resolve(
    onSuccess: user => UpdateUI(user),
    onError: error => ShowError($"User not found: {error}")
);
```

### Chaining Storage Operations

```csharp
// Load, modify, and save with proper error handling
var result = await store.Get<UserProfile>(userId)
    .BindAsync(async profile => {
        profile.LastAccessed = DateTime.UtcNow;
        return await store.Upsert(userId, profile);
    })
    .SelectAsync(async _ => await NotifyUserActivity(userId));

if (!result) {
    logger.LogWarning("Failed to update user activity");
}
```

### Message Handling

```csharp
public async Task<Result<Unit>> HandleMessage(Message message)
{
    return await Result.TryAsync(async () => {
        var data = DeserializeMessage(message);
        await ProcessData(data);
        return Result.Success();
    }, "Failed to process message");
}
```

## Best Practices

### When to Use Result<T>

✅ **Good Use Cases:**
- Domain operations that can fail in expected ways
- I/O operations where failures are common
- Validation operations
- Operations you want to chain functionally
- Public API methods where exceptions would be surprising

❌ **When NOT to Use:**
- Constructor failures (use exceptions)
- Programming errors (null arguments, out of bounds)
- Fatal errors that should stop execution
- Performance-critical hot paths (Result<T> has overhead)

### Error vs Failure

Use **Error** for expected domain failures:
```csharp
if (user == null)
    return Result.Error<User>("User not found");

if (!user.IsActive)
    return Result.Error<User>("User account is inactive");
```

Use **Failure** for unexpected exceptions:
```csharp
try {
    return Result.Success(await LoadData());
} catch (Exception ex) {
    return Result.Failure<Data>(ex);
}

// Or simply use Try methods which handle this automatically
return await Result.TryAsync(() => LoadData());
```

### Async Patterns

Always prefer async methods when dealing with I/O:

```csharp
// ✅ Good - fully async
public async Task<Result<Data>> LoadDataAsync()
{
    return await Result.TryAsync(async () => {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsAsync<Data>();
    });
}

// ❌ Avoid - blocks async operation
public Result<Data> LoadData()
{
    return Result.Try(() => {
        var task = httpClient.GetAsync(url);
        return task.Result; // Blocking!
    });
}
```

### Composition Over Nesting

Use extension methods to avoid nested handling:

```csharp
// ✅ Good - composed operations
var result = await GetUserId()
    .BindAsync(id => LoadUser(id))
    .BindAsync(user => ValidatePermissions(user))
    .SelectAsync(user => CreateSession(user));

// ❌ Avoid - nested if statements
var userIdResult = await GetUserId();
if (userIdResult.Successful) {
    var userResult = await LoadUser(userIdResult.ValueOrDefault());
    if (userResult.Successful) {
        var validResult = await ValidatePermissions(userResult.ValueOrDefault());
        // ... more nesting
    }
}
```

## Performance Considerations

The Result pattern has some overhead compared to raw exceptions or nullable returns:

- **Memory**: Each Result<T> is a record type with allocations
- **CPU**: Pattern matching and method calls have slight overhead
- **Async**: Async operations already have overhead, Result adds minimal extra

For most applications, this overhead is negligible compared to the benefits of explicit error handling and functional composition. However, in hot paths or performance-critical code, consider using traditional error handling.

## Migration Strategy

When adopting Result<T> in existing codebases:

1. **Start with boundaries**: Use Result<T> at service/API boundaries first
2. **Wrap existing code**: Use Try methods to wrap exception-throwing code
3. **Gradual adoption**: Don't rewrite everything at once
4. **Keep exceptions for truly exceptional cases**: Fatal errors, programming mistakes
5. **Document conventions**: Make it clear when to use Result<T> vs exceptions

## Summary

The Result pattern provides a powerful, pragmatic approach to error handling that:
- Makes errors explicit in method signatures
- Enables functional composition of operations
- Distinguishes between expected and unexpected failures
- Integrates seamlessly with async/await
- Improves code clarity and maintainability

Use it where it makes sense, not as a dogmatic replacement for all error handling. The goal is cleaner, more maintainable code that makes error handling explicit and composable.