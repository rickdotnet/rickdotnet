# RickUtils Reference

## Overview

RickUtils provides a collection of utility functions commonly needed in distributed systems and microservices. These utilities focus on ID generation, encoding, hashing, and serialization - fundamental operations for building robust distributed applications.

## Base32 Encoding

Base32 encoding is used throughout Aether for generating human-readable, URL-safe identifiers. It's preferred over Base64 for IDs because it:
- Uses only uppercase letters and numbers (no special characters)
- Avoids ambiguous characters (no 0/O or 1/I confusion)
- Is case-insensitive friendly
- Works well in URLs without encoding

### Basic Base32 Operations

```csharp
using RickDotNet.Base.Utils;

// Encode bytes to Base32
byte[] data = { 0x48, 0x65, 0x6C, 0x6C, 0x6F };
string encoded = RickUtils.ToBase32(data);
// Result: "JBSWY3DP"

// Using with spans for efficiency
ReadOnlySpan<byte> span = stackalloc byte[] { 1, 2, 3, 4, 5 };
string result = RickUtils.ToBase32(span);
```

## ID Generation

### Random Base32 IDs

Generate cryptographically secure random identifiers:

```csharp
// Generate a 20-character random ID
string id = RickUtils.RandomBase32(20);
// Example: "X7K4M9P2N5Q8R3T6V1W2"

// Generate different length IDs for different purposes
string shortId = RickUtils.RandomBase32(8);   // "A3K7M2P5"
string mediumId = RickUtils.RandomBase32(16); // "B4N8Q2W5E9R7T1Y6"
string longId = RickUtils.RandomBase32(32);   // For high-security tokens
```

### Use Cases for Random IDs

```csharp
public class Session
{
    public string Id { get; } = RickUtils.RandomBase32(16);
    public DateTime Created { get; } = DateTime.UtcNow;
}

public class ApiKey
{
    public string Key { get; } = RickUtils.RandomBase32(32);
    public string Secret { get; } = RickUtils.RandomBase32(48);
}

public class CorrelationId
{
    public static string Generate() => RickUtils.RandomBase32(12);
}
```

## Hashing

SHA256 hashing with automatic Base32 encoding for content-addressable storage:

### Basic Hashing

```csharp
// Hash any JSON-serializable object
var user = new { 
    Name = "Alice", 
    Email = "alice@example.com" 
};
string hash = RickUtils.Hash(user);
// Produces consistent Base32-encoded SHA256 hash

// Hash for content addressing
public class Document
{
    public string Content { get; set; }
    public string ContentHash => RickUtils.Hash(Content);
}
```

### Content-Addressable Storage Pattern

```csharp
public class ContentStore
{
    private readonly IStore store;
    
    public async Task<Result<string>> Save<T>(T content) where T : notnull
    {
        string id = RickUtils.Hash(content);
        
        // Content is immutable - if hash exists, content exists
        var existing = await store.Get<T>(id);
        if (existing.Successful)
            return Result.Success(id);
            
        return await store.Upsert(id, content);
    }
    
    public async Task<Result<T>> Load<T>(string contentHash)
    {
        return await store.Get<T>(contentHash);
    }
}
```

### Deduplication Example

```csharp
public class FileUploadService
{
    public async Task<string> UploadFile(byte[] fileData)
    {
        // Hash file content for deduplication
        string fileHash = RickUtils.Hash(fileData);
        
        // Check if file already exists
        if (await FileExists(fileHash))
        {
            Logger.Log("File already exists, skipping upload");
            return fileHash;
        }
        
        // Upload new file
        await SaveFile(fileHash, fileData);
        return fileHash;
    }
}
```

## Serialization

JSON serialization utilities optimized for performance:

```csharp
// Serialize to UTF8 bytes (efficient for network/storage)
var data = new { Message = "Hello", Timestamp = DateTime.UtcNow };
byte[] bytes = RickUtils.Serialize(data);

// Use with storage systems
public async Task SaveToCache<T>(string key, T value)
{
    byte[] serialized = RickUtils.Serialize(value);
    await cache.SetAsync(key, serialized);
}
```

## Real-World Examples from Aether

### Message IDs in Distributed Systems

```csharp
public class AetherMessage : IMessage
{
    public string Id { get; }
    public string CorrelationId { get; }
    public DateTime Timestamp { get; }
    
    public AetherMessage()
    {
        Id = RickUtils.RandomBase32(16);
        CorrelationId = RickUtils.RandomBase32(12);
        Timestamp = DateTime.UtcNow;
    }
}
```

### Event Sourcing with Content Hashing

```csharp
public class Event
{
    public string Id { get; } = RickUtils.RandomBase32(16);
    public string AggregateId { get; set; }
    public object Data { get; set; }
    public string DataHash => RickUtils.Hash(Data);
    
    public bool VerifyIntegrity(object data)
    {
        return DataHash == RickUtils.Hash(data);
    }
}
```

### Idempotency Keys

```csharp
public class IdempotentOperation
{
    private readonly IStore cache;
    
    public async Task<Result<T>> Execute<T>(
        Func<Task<T>> operation, 
        object request)
    {
        // Generate idempotency key from request
        string key = $"idempotent:{RickUtils.Hash(request)}";
        
        // Check if already processed
        var cached = await cache.Get<T>(key);
        if (cached.Successful)
            return cached;
        
        // Execute operation
        var result = await Result.TryAsync(operation);
        
        // Cache successful results
        if (result.Successful)
        {
            await cache.Upsert(key, result.ValueOrDefault(), 
                TimeSpan.FromHours(24));
        }
        
        return result;
    }
}
```

### Distributed Lock Keys

```csharp
public class DistributedLock
{
    private readonly string lockId;
    private readonly string owner;
    
    public DistributedLock(string resource)
    {
        lockId = $"lock:{RickUtils.Hash(resource)}";
        owner = RickUtils.RandomBase32(24); // Unique owner ID
    }
    
    public async Task<bool> AcquireAsync(TimeSpan duration)
    {
        // Use owner ID to ensure only we can release
        return await TrySetLock(lockId, owner, duration);
    }
}
```

## Integration Patterns

### With Result Pattern

```csharp
public Result<string> GenerateSecureToken(int length = 32)
{
    return Result.Try(() => 
    {
        if (length <= 0 || length > 256)
            return Result.Error<string>("Invalid token length");
            
        return Result.Success(RickUtils.RandomBase32(length));
    });
}
```

### With Storage Systems

```csharp
public class CacheKeyBuilder
{
    public static string BuildKey(params object[] parts)
    {
        var combined = string.Join(":", parts);
        return RickUtils.Hash(combined);
    }
    
    public static string BuildUserKey(int userId, string operation)
    {
        return $"user:{userId}:{RickUtils.Hash(operation)}";
    }
}
```

### With Message Queues

```csharp
public class MessageRouter
{
    public string GetQueueName<T>(T message) where T : notnull
    {
        // Use hash for consistent routing
        string hash = RickUtils.Hash(message);
        int bucket = Math.Abs(hash.GetHashCode()) % 10;
        return $"queue-{bucket}";
    }
    
    public string GetPartitionKey<T>(T message) where T : notnull
    {
        // Consistent partition assignment
        return RickUtils.Hash(message).Substring(0, 8);
    }
}
```

## Performance Considerations

### Base32 Encoding
- **Efficiency**: ~63% space efficiency (5 bits per character)
- **Speed**: Optimized for small strings (IDs, keys)
- **Memory**: Stack-allocated for small buffers

### Random Generation
- **Security**: Uses `RandomNumberGenerator` (cryptographically secure)
- **Performance**: Slower than `Random` but necessary for security
- **Caching**: Consider caching for non-security use cases

### Hashing
- **Algorithm**: SHA256 (balanced security/performance)
- **Caching**: Hash results are deterministic - cache when possible
- **Serialization**: JSON serialization overhead for complex objects

## Best Practices

### ID Generation

✅ **DO:**
- Use appropriate lengths (8-12 for internal, 16-24 for external, 32+ for secrets)
- Store IDs as strings in databases for readability
- Use consistent lengths within the same domain

❌ **DON'T:**
- Use sequential IDs in distributed systems
- Expose internal IDs in public APIs
- Use short IDs for security tokens

### Hashing

✅ **DO:**
- Use for content-addressable storage
- Cache hash results for expensive operations
- Verify data integrity with hashes

❌ **DON'T:**
- Use for password hashing (use proper password hashers)
- Hash sensitive data without encryption
- Assume hash uniqueness for security

### Serialization

✅ **DO:**
- Use `Serialize` for network/storage operations
- Consider compression for large objects
- Handle serialization errors appropriately

❌ **DON'T:**
- Serialize circular references
- Store serialized data without versioning
- Assume serialization format stability

## Common Patterns

### Generating Unique Identifiers

```csharp
public class EntityBase
{
    public string Id { get; } = RickUtils.RandomBase32(16);
    public DateTime CreatedAt { get; } = DateTime.UtcNow;
    public string ETag => RickUtils.Hash(new { Id, CreatedAt });
}
```

### Request/Response Correlation

```csharp
public class RequestContext
{
    public string RequestId { get; } = RickUtils.RandomBase32(12);
    public string SessionId { get; set; }
    public string CorrelationId => RickUtils.Hash(new { RequestId, SessionId });
}
```

### Cache Key Generation

```csharp
public static class CacheKeys
{
    public static string User(int id) => $"user:{id}";
    public static string UserData(int id, string dataType) => 
        $"user:{id}:{RickUtils.Hash(dataType)}";
    public static string Query<T>(T parameters) where T : notnull => 
        $"query:{typeof(T).Name}:{RickUtils.Hash(parameters)}";
}
```

## Summary

RickUtils provides essential utilities for distributed systems:
- **Base32**: Human-readable, URL-safe encoding
- **Random IDs**: Cryptographically secure identifier generation
- **Hashing**: Content addressing and integrity verification
- **Serialization**: Efficient data serialization for storage/network

These utilities form the foundation for building robust, scalable distributed systems with proper identification, deduplication, and data integrity.