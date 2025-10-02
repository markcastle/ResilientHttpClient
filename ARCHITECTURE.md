# Architecture Decisions

This document explains key architecture and design decisions made in ResilientHttpClient, particularly those that differ from typical .NET library patterns. These decisions were intentionally made to optimize for **Unity game engine compatibility** and **simplicity**.

---

## 1. Per-Instance Circuit Breaker State

###  Decision
Circuit breaker state (`_failureCount`, `_circuitOpenTime`) is stored per-instance of `ResilientHttpClient`, not globally or per-endpoint.

### 📋 Rationale

**For Unity (Primary Use Case):**
- Unity applications typically create **ONE** `ResilientHttpClient` instance for the entire application
- This single instance manages all API calls to a specific service
- Per-instance state works perfectly for this pattern

**Example:**
```csharp
// Unity pattern - single instance
private static readonly IResilientHttpClient _apiClient = 
    ResilientHttpClientFactory.CreateClient("https://api.yourgame.com");
```

### ⚠️ Implications

If you create multiple instances pointing to the same endpoint:
```csharp
var client1 = ResilientHttpClientFactory.CreateClient("https://api.com");
var client2 = ResilientHttpClientFactory.CreateClient("https://api.com");
// client1 and client2 have SEPARATE circuit breakers
```

This is intentional and expected for the Unity use case. If you need shared circuit breaker state across instances, you should reuse the same instance (which is already recommended for other reasons - see Best Practices).

### 🔮 Future
If shared state becomes a requirement, we may add an optional "shared circuit breaker pool" in a future version.

---

## 2. Using `HttpRequestMessage.Properties` (Obsolete API)

### Decision
We use `HttpRequestMessage.Properties` to store per-request policies, even though this API is marked obsolete in .NET 5+.

### 📋 Rationale

**For .NET Standard 2.1 Compatibility:**
- Unity uses .NET Standard 2.1, which doesn't have the newer `Options` API
- `Properties` is the only way to attach metadata to requests in .NET Standard 2.1
- The newer `HttpRequestOptions` API was introduced in .NET 5

**Trade-off:**
- ✅ Works in Unity (.NET Standard 2.1)
- ✅ Works in .NET Framework
- ⚠️ Shows obsolete warnings in .NET 5+ projects
- ⚠️ Will need migration path for .NET 5+ in future

###  Current Code
```csharp
// ResilientHttpClient.Core targets .NET Standard 2.1
request.Properties[PolicyPropertyKey] = policy; // Only option available
```

###  Future Migration Path
For v2.0 or when dropping .NET Standard 2.1:
```csharp
#if NETSTANDARD2_1 || NETFRAMEWORK
request.Properties[PolicyPropertyKey] = policy;
#else
request.Options.Set(new HttpRequestOptionsKey<RequestPolicy>(PolicyPropertyKey), policy);
#endif
```

---

## 3. Static Retry Delay (No Exponential Backoff)

### Decision
Retry delay is a fixed `TimeSpan`, not exponential.

### 📋 Rationale

**Simplicity First:**
- Fixed delay is easier to understand and configure
- Sufficient for most Unity game scenarios
- Avoids complexity of exponential backoff + jitter calculations

**Current Implementation:**
```csharp
// Simple, predictable behavior
var options = new ResilientHttpClientOptions
{
    RetryDelay = TimeSpan.FromSeconds(1) // Same delay for every retry
};
```

###  Known Limitation

Fixed delays can cause "thundering herd" problems when many clients retry simultaneously. This is typically not an issue for:
- Single-player games (only one client per game instance)
- Low-traffic scenarios
- APIs that can handle concurrent retries

###  Planned Feature
Exponential backoff with jitter is planned for v1.1 as an opt-in feature:
```csharp
var options = new ResilientHttpClientOptions
{
    RetryStrategy = RetryStrategy.ExponentialBackoff,
    InitialRetryDelay = TimeSpan.FromSeconds(1),
    MaxRetryDelay = TimeSpan.FromSeconds(30)
};
```

See `TASK.md` Epic 3 for implementation details.

---

## 4. Large Interface (30+ Methods)

###  Decision
`IResilientHttpClient` mirrors the entire `HttpClient` API, resulting in a large interface (~30 methods).

### 📋 Rationale

**Drop-in Replacement:**
- Goal: Replace `HttpClient` without changing calling code
- Provides all HttpClient methods: GET, POST, PUT, DELETE, SendAsync, etc.
- All overloads supported (string URI, Uri object, CancellationToken, HttpCompletionOption)

**Example:**
```csharp
// Before: Using HttpClient
HttpClient client = new HttpClient();
var response = await client.GetAsync("/api/data");

// After: Using ResilientHttpClient (no code changes needed!)
IResilientHttpClient client = ResilientHttpClientFactory.CreateClient();
var response = await client.GetAsync("/api/data"); // Same method signature
```

### ⚠️ Violates Interface Segregation Principle (ISP)

Yes, this is a conscious violation of ISP. **Why it's acceptable:**
- **Consistency** with HttpClient is more valuable than ISP compliance
- **Discoverability** - developers find familiar methods
- **Migration** path from HttpClient is seamless

###  Alternative Not Chosen
We could have created smaller, focused interfaces:
```csharp
interface IHttpGet { Task<HttpResponseMessage> GetAsync(...); }
interface IHttpPost { Task<HttpResponseMessage> PostAsync(...); }
// etc.
```

**Why we didn't:** Would break the "drop-in replacement" goal.

---

## 5. No Request Disposal in Wrapper Methods

###  Decision
Wrapper methods (GET, POST, PUT, DELETE) don't use `using` statements to dispose requests.

### 📋 Rationale

**Content Cloning Requirements:**
- Retry logic needs to clone requests (including their content)
- Content can only be read once - disposal would prevent retries
- Requests are disposed inside `SendAsync` after all retry attempts

**Before (Broken):**
```csharp
public Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken ct)
{
    using var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
    return SendAsync(request, ct); // ❌ Request disposed before SendAsync completes!
}
```

**After (Fixed):**
```csharp
public Task<HttpResponseMessage> PostAsync(Uri uri, HttpContent content, CancellationToken ct)
{
    var request = new HttpRequestMessage(HttpMethod.Post, uri) { Content = content };
    return SendAsync(request, ct); // ✅ Disposal handled inside SendAsync
}
```

###  Memory Management

- Original request: NOT disposed by library (user might reuse it)
- Cloned requests: Disposed after each retry attempt
- Final request: NOT disposed if response is successful (response references it)

This matches `HttpClient` behavior.

---

## Summary

| Decision | Reason | Trade-off |
|----------|--------|-----------|
| Per-instance circuit breaker | Unity single-instance pattern | No shared state across instances |
| Use `Properties` API | .NET Standard 2.1 compatibility | Obsolete warnings in .NET 5+ |
| Static retry delay | Simplicity | No exponential backoff (yet) |
| Large interface | Drop-in HttpClient replacement | Violates ISP |
| No `using` in wrappers | Enable content cloning for retries | Slightly different disposal semantics |

All decisions prioritize **Unity compatibility** and **simplicity** over enterprise-grade features. Advanced features can be added in future versions without breaking changes.
