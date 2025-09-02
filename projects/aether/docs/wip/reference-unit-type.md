# Unit Type Reference

## Overview

The Unit type is a simple but powerful construct that represents "no value" in a type-safe way. It's the functional programming equivalent of `void`, but unlike `void`, Unit is an actual type that can be used in generic contexts, particularly with `Result<T>`.

## Why Unit?

In C#, `void` is not a real type - you cannot have a `Task<void>`, `Result<void>`, or `List<void>`. This creates asymmetry in APIs and forces special handling for operations that don't return values. Unit solves this by providing a real type that represents "no meaningful value."

## Definition

```csharp
public struct Unit
{
    public static readonly Unit Default = new();
}
```

Unit is:
- A value type (struct) for efficiency
- Has only one possible value (`Unit.Default`)
- Lightweight (zero memory overhead)
- Immutable by design

## Core Use Cases

### 1. Result<Unit> for Void Operations

The most common use is with Result pattern for operations that succeed/fail without returning data:

```csharp
// Instead of this (not possible)
// Result<void> DeleteUser(int id)

// Use this
Result<Unit> DeleteUser(int id)
{
    if (!UserExists(id))
        return Result.Error<Unit>("User not found");
    
    PerformDelete(id);
    return Result.Success();  // Returns Result<Unit>
}
```

### 2. Commands in CQRS

Commands typically don't return values, only success/failure:

```csharp
public interface ICommand : IMessage { }

public interface ICommandHandler<TCommand> where TCommand : ICommand
{
    Task<Result<Unit>> Handle(TCommand command, CancellationToken ct);
}

public class DeleteUserCommand : ICommand
{
    public int UserId { get; set; }
}

public class DeleteUserCommandHandler : ICommandHandler<DeleteUserCommand>
{
    public async Task<Result<Unit>> Handle(DeleteUserCommand command, CancellationToken ct)
    {
        var deleted = await database.DeleteUser(command.UserId);
        
        return deleted > 0 
            ? Result.Success() 
            : Result.Error<Unit>("User not found");
    }
}
```

### 3. Event Handlers

Event handlers typically perform side effects without returning values:

```csharp
public interface IEventHandler<TEvent> where TEvent : IEvent
{
    Task<Result<Unit>> Handle(TEvent evt, CancellationToken ct);
}

public class UserDeletedEventHandler : IEventHandler<UserDeletedEvent>
{
    public async Task<Result<Unit>> Handle(UserDeletedEvent evt, CancellationToken ct)
    {
        return await Result.TryAsync(async () =>
        {
            await emailService.SendGoodbyeEmail(evt.UserEmail);
            await auditLog.LogDeletion(evt.UserId);
            return Result.Success();
        });
    }
}
```

## Working with Result<Unit>

### Creating Success Results

```csharp
// All of these create Result<Unit>.Success
Result<Unit> result1 = Result.Success();
Result<Unit> result2 = Result.Success(Unit.Default);
Result<Unit> result3 = new Result<Unit>.Success(Unit.Default);

// Async versions
Task<Result<Unit>> asyncResult1 = Result.SuccessTask();
Task<Result<Unit>> asyncResult2 = Task.FromResult(Result.Success());
```

### Creating Error Results

```csharp
// Error results for Unit operations
Result<Unit> error1 = Result.Error("Operation failed");
Result<Unit> error2 = Result.Failure(new InvalidOperationException());

// Async versions
Task<Result<Unit>> asyncError = Result.ErrorTask("Failed");
```

### Pattern Matching

```csharp
Result<Unit> result = await PerformOperation();

var message = result switch
{
    Result<Unit>.Success => "Operation completed successfully",
    Result<Unit>.Error error => $"Failed: {error.ErrorMessage}",
    Result<Unit>.Failure failure => $"Exception: {failure.Exception.Message}",
    _ => "Unknown result"
};
```

## Real-World Examples from Aether

### Message Publishing

```csharp
public interface IMessageHub
{
    Task<Result<Unit>> Publish<T>(T message) where T : IMessage;
}

public class NatsHub : IMessageHub
{
    public async Task<Result<Unit>> Publish<T>(T message) where T : IMessage
    {
        return await Result.TryAsync(async () =>
        {
            var subject = GetSubject<T>();
            var data = Serialize(message);
            await connection.PublishAsync(subject, data);
            return Result.Success();
        }, $"Failed to publish {typeof(T).Name}");
    }
}
```

### Storage Operations

```csharp
public class StorageService
{
    public async Task<Result<Unit>> Delete(string key)
    {
        return await Result.TryAsync(async () =>
        {
            var exists = await store.Exists(key);
            if (!exists)
                return Result.Error<Unit>("Key not found");
            
            await store.Remove(key);
            return Result.Success();
        });
    }
    
    public async Task<Result<Unit>> Clear()
    {
        return await Result.TryAsync(async () =>
        {
            await store.ClearAll();
            return Result.Success();
        }, "Failed to clear storage");
    }
}
```

### Validation Operations

```csharp
public class ValidationService
{
    public Result<Unit> ValidateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return Result.Error<Unit>("Email is required");
            
        if (!email.Contains('@'))
            return Result.Error<Unit>("Invalid email format");
            
        return Result.Success();
    }
    
    public async Task<Result<Unit>> ValidateUserData(UserData data)
    {
        // Chain multiple validations
        return ValidateEmail(data.Email)
            .Bind(_ => ValidateAge(data.Age))
            .BindAsync(async _ => await ValidateUniqueEmail(data.Email));
    }
}
```

## Patterns and Best Practices

### Chaining Void Operations

```csharp
public async Task<Result<Unit>> ProcessOrder(Order order)
{
    return await ValidateOrder(order)
        .BindAsync(_ => DeductInventory(order.Items))
        .BindAsync(_ => ChargePayment(order.Payment))
        .BindAsync(_ => SendConfirmationEmail(order.CustomerEmail))
        .OnSuccessAsync(_ => logger.LogInformation("Order processed"));
}
```

### Converting to Valued Results

Sometimes you need to convert Result<Unit> to a valued result:

```csharp
public async Task<Result<OrderConfirmation>> PlaceOrder(Order order)
{
    return await ProcessOrder(order)  // Returns Result<Unit>
        .SelectAsync(async _ => 
        {
            // After successful processing, create confirmation
            return new OrderConfirmation
            {
                OrderId = order.Id,
                Timestamp = DateTime.UtcNow,
                Status = "Confirmed"
            };
        });
}
```

### Side Effects with Unit

```csharp
public class SideEffectService
{
    public async Task<Result<Unit>> ExecuteWithSideEffects(Data data)
    {
        return await Result.TryAsync(async () =>
        {
            // Primary operation
            await ProcessData(data);
            
            // Side effects (non-critical)
            _ = Task.Run(() => UpdateCache(data));
            _ = Task.Run(() => SendMetrics(data));
            
            return Result.Success();
        });
    }
}
```

### Unit in Middleware

```csharp
public class LoggingMiddleware
{
    public async Task<Result<Unit>> Execute(
        RequestContext context,
        Func<Task<Result<Unit>>> next)
    {
        logger.LogInformation($"Request: {context.RequestId}");
        
        var result = await next();
        
        result.Resolve(
            onSuccess: _ => logger.LogInformation("Request succeeded"),
            onError: error => logger.LogError($"Request failed: {error}")
        );
        
        return result;
    }
}
```

## Testing with Unit

### Simple Assertions

```csharp
[Fact]
public async Task DeleteUser_WhenUserExists_ReturnsSuccess()
{
    // Arrange
    var service = new UserService();
    
    // Act
    var result = await service.DeleteUser(123);
    
    // Assert
    Assert.True(result.Successful);
    Assert.IsType<Result<Unit>.Success>(result);
}

[Fact]
public async Task DeleteUser_WhenUserNotFound_ReturnsError()
{
    // Arrange
    var service = new UserService();
    
    // Act
    var result = await service.DeleteUser(999);
    
    // Assert
    Assert.False(result.Successful);
    var error = Assert.IsType<Result<Unit>.Error>(result);
    Assert.Equal("User not found", error.ErrorMessage);
}
```

### Mock Setups

```csharp
// Moq example
mock.Setup(x => x.SaveData(It.IsAny<Data>()))
    .ReturnsAsync(Result.Success());

mock.Setup(x => x.DeleteData(It.IsAny<string>()))
    .ReturnsAsync(Result.Error<Unit>("Not found"));

// Verify calls
mock.Verify(x => x.SaveData(It.IsAny<Data>()), Times.Once);
```

## Common Patterns

### Fire and Forget with Logging

```csharp
public async Task<Result<Unit>> FireAndForget(Func<Task> operation, string operationName)
{
    _ = Task.Run(async () =>
    {
        try
        {
            await operation();
            logger.LogInformation($"{operationName} completed");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, $"{operationName} failed");
        }
    });
    
    return Result.Success();
}
```

### Batch Operations

```csharp
public async Task<Result<Unit>> ProcessBatch(IEnumerable<Item> items)
{
    var results = await Task.WhenAll(
        items.Select(item => ProcessItem(item))
    );
    
    var failures = results.Where(r => r.NotSuccessful).ToList();
    
    if (failures.Any())
    {
        var errors = string.Join(", ", failures.Select(f => f.ToString()));
        return Result.Error<Unit>($"Batch processing failed: {errors}");
    }
    
    return Result.Success();
}
```

### Retries with Unit

```csharp
public async Task<Result<Unit>> RetryOperation(
    Func<Task<Result<Unit>>> operation,
    int maxRetries = 3)
{
    for (int i = 0; i < maxRetries; i++)
    {
        var result = await operation();
        if (result.Successful)
            return result;
            
        if (i < maxRetries - 1)
            await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, i)));
    }
    
    return Result.Error<Unit>($"Operation failed after {maxRetries} retries");
}
```

## Integration with Aether Components

### Workers (Actors)

```csharp
public class EmailWorker : Worker
{
    public async Task<Result<Unit>> SendEmail(EmailCommand command)
    {
        return await Result.TryAsync(async () =>
        {
            await emailClient.SendAsync(command.To, command.Subject, command.Body);
            await PublishEvent(new EmailSentEvent { EmailId = command.Id });
            return Result.Success();
        });
    }
}
```

### Endpoints

```csharp
public class HealthEndpoint : Endpoint
{
    [HttpGet("/health")]
    public async Task<Result<Unit>> CheckHealth()
    {
        var checks = await Task.WhenAll(
            CheckDatabase(),
            CheckMessageQueue(),
            CheckCache()
        );
        
        return checks.All(c => c.Successful)
            ? Result.Success()
            : Result.Error<Unit>("Health check failed");
    }
}
```

## Performance Considerations

Unit is extremely lightweight:
- **Size**: Zero bytes (empty struct)
- **Allocation**: Stack-allocated, no heap allocation
- **Comparison**: Instant (all Units are equal)
- **Pattern Matching**: Efficient with modern C# compilers

Result<Unit> has the same overhead as any Result<T>:
- One allocation for the Result record
- Pattern matching overhead
- Method call overhead for extensions

For hot paths, consider:
- Caching `Result.Success()` (already done in the framework)
- Using `ValueTask<Result<Unit>>` for synchronous paths
- Avoiding excessive chaining in tight loops

## Summary

The Unit type enables:
- Type-safe representation of "no value"
- Uniform handling of all operations through Result<T>
- Functional composition of void operations
- Clear distinction between success/failure without data

Use Unit when:
- Operations succeed/fail without returning data
- Implementing command handlers in CQRS
- Building middleware chains
- Composing multiple void operations
- Need type safety for "void" operations

Unit brings symmetry and composability to operations that traditionally would use void, making your APIs more consistent and your code more functional.