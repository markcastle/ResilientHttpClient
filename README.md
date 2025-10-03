# ResilientHttpClient 🚀

![Coverage](https://img.shields.io/badge/coverage-96.7%25-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blueviolet?style=flat-square)
![Tests](https://img.shields.io/badge/tests-137%20passing-brightgreen?style=flat-square)
![Version](https://img.shields.io/badge/version-1.0.0-blue?style=flat-square)

🎉 **Welcome to ResilientHttpClient!**

A drop-in replacement for HttpClient that adds common resiliency patterns such as circuit breaker, retry, and timeout. This library is compatible with .NET Standard 2.1 and can be used in Unity projects.

---

## ✨ Features

- **🛡️ Circuit Breaker Pattern**: Prevents cascading failures by stopping requests after a certain number of failures.
- **🔁 Retry Pattern**: Automatically retries transient failures with configurable retry count and delay.
- **⏱️ Timeout Handling**: Properly handles timeouts and treats them as transient failures.
- **🔄 Drop-in Replacement**: Implements the same interface as HttpClient, making it easy to replace existing code.
- **📦 No External Dependencies**: Uses only native .NET Standard 2.1 features, making it compatible with Unity.
- **✅ Well-Tested**: Comprehensive unit tests ensure reliability and correct behavior.
- **🧰 Complete API Coverage**: Supports all HttpClient methods including GetStringAsync for direct string responses.
- **🎯 Per-Request Policies**: Customize resilience behavior for individual requests using a fluent interface.
- **🔌 Dependency Injection**: First-class support for ASP.NET Core DI with named clients and configuration.

---

## 🔧 Recent Improvements

### v0.9.0 - Pre-Release (October 2025)

**🆕 New Features:**
- **Dependency Injection Package** - Added `ResilientHttpClient.Extensions.DependencyInjection` for seamless ASP.NET Core integration
  - Simple registration: `services.AddResilientHttpClient()`
  - Named clients support: `services.AddNamedResilientHttpClient("GitHub", "https://api.github.com")`
  - Configurable lifetimes (Singleton/Scoped/Transient)
  - 25 comprehensive tests (all passing)

**🐛 Critical Bug Fixes:**
- **Fixed content cloning in retry scenarios** - POST/PUT requests with content now properly clone the HttpContent during retries, preventing "Cannot access a disposed object" errors. Previously, content was shared between retry attempts, causing failures when the content stream was already consumed.
  - Added async content cloning with full header preservation
  - Improved request disposal logic to prevent premature cleanup
  - Added 8 comprehensive tests covering all retry scenarios with content

**📚 Documentation Enhancements:**
- **Added Best Practices section** - Critical guidance on instance reuse patterns to avoid socket exhaustion
- **Created ARCHITECTURE.md** - Detailed explanation of design decisions and trade-offs
- **Enhanced XML documentation** - Factory methods now include warnings and usage examples

**📊 Quality Improvements:**
- Coverage increased from 95.2% to **96.7%** line coverage
- Added 8 new tests specifically for content cloning scenarios
- All 112 tests passing

**Why this matters:** If you're using POST or PUT requests with retry logic, this fix prevents silent failures where retry attempts would fail due to content already being consumed. Upgrade recommended for all users making non-GET requests.

---

## ⚡ Quickstart

```csharp
// Create once, reuse for all requests (see Best Practices below)
var client = ResilientHttpClientFactory.CreateClient();
var response = await client.GetAsync("https://api.example.com/data");
```

**⚠️ Important:** See [Best Practices](#️-best-practices) below for proper instance reuse patterns.

---

## 📦 Installation

### For .NET Projects

1. Add the `ResilientHttpClient.Core.dll` to your project references.
2. Make sure you have a reference to `System.Net.Http` in your project.

### For Unity Projects

1. Place the `ResilientHttpClient.Core.dll` in your `Assets/Plugins` folder.
2. No additional dependencies are required.

---

## 🛠️ Building from Source

The project includes two batch files to simplify building:

- **build.bat**: Builds the project in Release mode and outputs the DLL location.
- **build-and-copy.bat**: Builds the project and optionally copies the DLL to your Unity project.

To build and copy to a Unity project, you have two options:

1. Pass the path as a command-line argument:
   ```
   build-and-copy.bat "C:\Path\To\Your\UnityProject"
   ```

2. Create a `.env` file in the project root with your Unity project path:
   ```
   UNITY_PROJECT_PATH=C:\Path\To\Your\UnityProject
   ```
   Then simply run `build-and-copy.bat` without arguments.

A sample `.env.example` file is provided that you can copy and modify.

---

## 🎯 Demo & Examples

Want to see ResilientHttpClient in action? We've included a simple console demo that showcases all the key features!

### Running the Demo

```bash
cd ResilientHttpClient.Demo
dotnet run
```

Or from the solution root:

```bash
dotnet run --project ResilientHttpClient.Demo
```

### What the Demo Shows

The demo includes 4 complete examples:

1. **Basic GET Request** - Simple usage with the factory pattern
2. **Retry Logic** - Automatic retries on transient failures
3. **Custom Policies** - Per-request configuration using the fluent API
4. **Circuit Breaker** - Protection against cascading failures

Each example includes:
- Clear console output with success/failure indicators
- Detailed code comments explaining what's happening
- Real-world API calls (uses JSONPlaceholder for testing)
- Educational notes about when features activate

**Perfect for:** Learning the library, testing features, or copying code snippets into your project!

---

## 🚦 Usage

### Basic Usage

```csharp
// Create a client with default options
var client = ResilientHttpClientFactory.CreateClient();

// Make requests just like with HttpClient
var response = await client.GetAsync("https://api.example.com/data");
```

### With Base Address

```csharp
// Create a client with a base address
var client = ResilientHttpClientFactory.CreateClient("https://api.example.com");

// Now you can use relative URLs
var response = await client.GetAsync("/data");
```

### With Custom Options

```csharp
// Configure custom resilience options
var options = new ResilientHttpClientOptions
{
    MaxRetries = 5,
    RetryDelay = TimeSpan.FromSeconds(2),
    MaxFailures = 10,
    CircuitResetTime = TimeSpan.FromMinutes(1)
};

// Create a client with custom options
var client = ResilientHttpClientFactory.CreateClient(options);

// Or with a base address
var client = ResilientHttpClientFactory.CreateClient("https://api.example.com", options);
```

### Getting String Content Directly

```csharp
// Get response content as a string directly
var content = await client.GetStringAsync("https://api.example.com/data");

// With cancellation token
var cts = new CancellationTokenSource();
var content = await client.GetStringAsync("https://api.example.com/data", cts.Token);
```

### Per-Request Resilience Policies

You can customize resilience behavior for individual requests using the fluent interface:

```csharp
// Create a request with custom resilience policy
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data")
    .WithPolicy(policy => policy
        .WithMaxRetries(10)
        .WithRetryDelay(TimeSpan.FromMilliseconds(200)));

// Send the request
var response = await client.SendAsync(request);
```

#### Bypassing the Circuit Breaker 📴

For critical requests that should be attempted even when the circuit is open:

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/critical-data")
    .WithPolicy(policy => policy.BypassCircuitBreaker());

var response = await client.SendAsync(request);
```

#### Disabling Retries 🚫🔁

For non-critical requests where immediate feedback is more important than success:

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/non-critical-data")
    .WithPolicy(policy => policy.DisableRetries());

var response = await client.SendAsync(request);
```

#### Combining Multiple Policy Options 🧩

You can combine multiple policy options for fine-grained control:

```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data")
    .WithPolicy(policy => policy
        .WithMaxRetries(2)
        .WithRetryDelay(TimeSpan.FromSeconds(5))
        .BypassCircuitBreaker());

var response = await client.SendAsync(request);
```

### Manual Creation

```csharp
// If you need more control, you can create the client manually
var httpClient = new HttpClient();
var resilientClient = new ResilientHttpClient(httpClient, options);
```

---

## 🔌 Dependency Injection (ASP.NET Core)

**NEW!** Use the `ResilientHttpClient.Extensions.DependencyInjection` package for seamless integration with Microsoft DI.

### Installation

```bash
# Install the DI extension package
dotnet add package ResilientHttpClient.Extensions.DependencyInjection
```

### Basic Registration

**Simple registration with defaults:**

```csharp
// In Program.cs or Startup.cs
services.AddResilientHttpClient();

// Inject and use
public class MyService
{
    private readonly IResilientHttpClient _client;
    
    public MyService(IResilientHttpClient client)
    {
        _client = client;
    }
}
```

**With base address and configuration:**

```csharp
services.AddResilientHttpClient("https://api.example.com", options =>
{
    options.MaxRetries = 5;
    options.RetryDelay = TimeSpan.FromSeconds(2);
    options.CircuitResetTime = TimeSpan.FromSeconds(60);
});
```

### Named Clients

Register multiple clients for different APIs:

```csharp
// Register multiple named clients
services.AddNamedResilientHttpClient("GitHub", "https://api.github.com", options =>
{
    options.MaxRetries = 3;
});

services.AddNamedResilientHttpClient("MyAPI", "https://myapi.com", options =>
{
    options.MaxRetries = 5;
    options.RetryDelay = TimeSpan.FromMilliseconds(500);
});

// Inject the factory and resolve by name
public class MyService
{
    private readonly IResilientHttpClient _githubClient;
    private readonly IResilientHttpClient _apiClient;
    
    public MyService(IResilientHttpClientFactory factory)
    {
        _githubClient = factory.CreateClient("GitHub");
        _apiClient = factory.CreateClient("MyAPI");
    }
    
    public async Task<string> GetGitHubRepo(string owner, string repo)
    {
        var response = await _githubClient.GetAsync($"/repos/{owner}/{repo}");
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    }
}
```

### Configuration from appsettings.json (Coming Soon)

```json
{
  "ResilientHttpClient": {
    "MaxRetries": 5,
    "RetryDelay": "00:00:02",
    "CircuitResetTime": "00:00:30",
    "MaxFailures": 10
  }
}
```

```csharp
services.Configure<ResilientHttpClientOptions>(
    Configuration.GetSection("ResilientHttpClient"));
services.AddResilientHttpClient();
```

### Lifetime Options

By default, the client is registered as a **Singleton** (recommended). You can change this:

```csharp
// Scoped lifetime (new instance per request)
services.AddResilientHttpClient(
    configure: options => { /* ... */ },
    lifetime: ServiceLifetime.Scoped);

// Transient lifetime (new instance every time)
services.AddResilientHttpClient(
    configure: options => { /* ... */ },
    lifetime: ServiceLifetime.Transient);
```

**⚠️ Note:** Singleton is recommended to avoid socket exhaustion. See [Best Practices](#️-best-practices) below.

---

## ⚠️ Best Practices

### **CRITICAL: Reuse Your ResilientHttpClient Instance**

**⚠️ WARNING:** Creating a new `ResilientHttpClient` for each request is an anti-pattern that can lead to socket exhaustion and DNS issues.

#### The Problem

When you create a new HttpClient instance (which ResilientHttpClient wraps), it creates its own connection pool. Creating many instances can:
- **Exhaust available sockets** - Leading to `SocketException` errors
- **Ignore DNS changes** - Your app won't pick up DNS updates
- **Waste resources** - Each instance manages its own connections

####  The Solution: Create Once, Reuse Forever

**For Unity Projects (Recommended Pattern):**

```csharp
public class ApiManager : MonoBehaviour
{
    // Create ONE instance as a static readonly field
    private static readonly IResilientHttpClient _apiClient = 
        ResilientHttpClientFactory.CreateClient("https://api.yourgame.com");

    public async Task<PlayerData> GetPlayerData(string playerId)
    {
        // Reuse the same instance for all requests
        var response = await _apiClient.GetAsync($"/players/{playerId}");
        response.EnsureSuccessStatusCode();
        
        var json = await response.Content.ReadAsStringAsync();
        return JsonUtility.FromJson<PlayerData>(json);
    }
    
    public async Task<bool> SubmitScore(int score)
    {
        var content = new StringContent($"{{\"score\":{score}}}", Encoding.UTF8, "application/json");
        var response = await _apiClient.PostAsync("/scores", content);
        return response.IsSuccessStatusCode;
    }
}
```

**For ASP.NET Core / Dependency Injection:**

```csharp
// In Startup.cs or Program.cs
public void ConfigureServices(IServiceCollection services)
{
    // Register as singleton - ONE instance for the entire application
    services.AddSingleton<IResilientHttpClient>(sp => 
        ResilientHttpClientFactory.CreateClient("https://api.example.com", new ResilientHttpClientOptions
        {
            MaxRetries = 3,
            CircuitResetTime = TimeSpan.FromSeconds(30)
        }));
}

// In your controller or service
public class MyService
{
    private readonly IResilientHttpClient _client;
    
    public MyService(IResilientHttpClient client)
    {
        _client = client; // Injected singleton instance
    }
    
    public async Task<Data> GetData()
    {
        return await _client.GetAsync("/data");
    }
}
```

**For Console Apps / Simple Projects:**

```csharp
class Program
{
    // Create ONE instance at the application level
    private static readonly IResilientHttpClient _client = 
        ResilientHttpClientFactory.CreateClient("https://api.example.com");

    static async Task Main(string[] args)
    {
        // Reuse the same instance throughout your application
        await FetchData();
        await FetchMoreData();
        await PostData();
    }
    
    static async Task FetchData()
    {
        var response = await _client.GetAsync("/endpoint");
        // ... handle response
    }
}
```

#### ❌ Don't Do This (Anti-Pattern)

```csharp
// BAD - Creates a new instance for every request!
public async Task<Data> GetData()
{
    var client = ResilientHttpClientFactory.CreateClient(); // ❌ Don't do this in a method!
    var response = await client.GetAsync("https://api.example.com/data");
    return response;
}
```

#### ✅ Do This Instead

```csharp
// GOOD - Reuse a single instance
private static readonly IResilientHttpClient _client = 
    ResilientHttpClientFactory.CreateClient("https://api.example.com");

public async Task<Data> GetData()
{
    var response = await _client.GetAsync("/data"); // ✅ Reuse the instance
    return response;
}
```

###  Thread Safety

`ResilientHttpClient` is **thread-safe** and designed to be shared across multiple threads. You can safely make concurrent requests from different threads using the same instance.

```csharp
// This is safe and recommended
var tasks = new[]
{
    _client.GetAsync("/endpoint1"),
    _client.GetAsync("/endpoint2"),
    _client.GetAsync("/endpoint3")
};

var responses = await Task.WhenAll(tasks);
```

---

## ⚙️ Understanding Resilience Options

The `ResilientHttpClient` provides several options to customize its behavior when dealing with failures. Here's a detailed explanation of each option:

### 🔁 Retry Policy Options

#### MaxRetries
**Default value**: 3  
**What it does**: Controls how many times the client will retry a failed request before giving up.

**In simple terms**: If your app tries to fetch data and the server is temporarily busy, this setting determines how many additional attempts your app will make before telling you it couldn't get the data.

**Example scenarios**:
- **Low value (1-2)**: Good for non-critical operations or when quick feedback is more important than success.
- **Medium value (3-5)**: Balanced approach for most API calls.
- **High value (6+)**: For critical operations where success is essential, even if it takes longer.

#### RetryDelay
**Default value**: 1 second  
**What it does**: Sets how long the client will wait between retry attempts.

**In simple terms**: This is the "cooling off" period between attempts. Like waiting a moment before trying to open a jammed door again.

**Example scenarios**:
- **Short delay (0.1-0.5s)**: For time-sensitive operations where quick retries are important.
- **Medium delay (1-3s)**: Good balance for most scenarios, giving the server time to recover.
- **Long delay (5s+)**: For scenarios where the server might need more time to recover, or to avoid overwhelming it.

**Note**: Longer delays mean your users wait longer for responses, but too short delays might not give the server enough time to recover.

### ⚡ Circuit Breaker Options

#### MaxFailures
**Default value**: 5  
**What it does**: Determines how many consecutive failures must occur before the circuit "opens" (temporarily stops all requests).

**In simple terms**: This is like a circuit breaker in your home. If too many failures happen in a row, the client stops trying to prevent further damage or resource waste.

**Example scenarios**:
- **Low value (2-3)**: Very sensitive, will "trip" quickly. Good for critical systems where you want to fail fast.
- **Medium value (5-10)**: Balanced approach that tolerates some failures but protects against sustained problems.
- **High value (15+)**: More tolerant of failures, good when occasional errors are normal or when the service is less reliable.

#### CircuitResetTime
**Default value**: 30 seconds  
**What it does**: Controls how long the circuit stays open before allowing a "test" request to check if the service has recovered.

**In simple terms**: After the circuit breaker trips, this is how long your app will wait before trying the service again.

**Example scenarios**:
- **Short time (5-15s)**: For services that recover quickly or when availability is critical.
- **Medium time (30-60s)**: Good balance for most services.
- **Long time (2min+)**: For services that take longer to recover or when you want to ensure stability before resuming normal operations.

### 🧩 Putting It All Together

These options work together to create a resilient HTTP client:

1. When a request fails with a transient error, the client will retry up to `MaxRetries` times, waiting `RetryDelay` between attempts.
2. If failures continue and reach `MaxFailures` consecutive failures, the circuit opens and all requests immediately fail with a "Circuit is open" exception.
3. After `CircuitResetTime` has passed, the circuit allows one test request through. If it succeeds, the circuit closes and normal operation resumes. If it fails, the circuit stays open for another `CircuitResetTime` period.

### 📝 Recommended Configurations

#### For Most Applications
```csharp
var options = new ResilientHttpClientOptions
{
    MaxRetries = 3,
    RetryDelay = TimeSpan.FromSeconds(1),
    MaxFailures = 5,
    CircuitResetTime = TimeSpan.FromSeconds(30)
};
```

#### For Critical Operations
```csharp
var options = new ResilientHttpClientOptions
{
    MaxRetries = 5,
    RetryDelay = TimeSpan.FromSeconds(2),
    MaxFailures = 3,
    CircuitResetTime = TimeSpan.FromSeconds(15)
};
```

#### For Background Operations
```csharp
var options = new ResilientHttpClientOptions
{
    MaxRetries = 10,
    RetryDelay = TimeSpan.FromSeconds(5),
    MaxFailures = 10,
    CircuitResetTime = TimeSpan.FromMinutes(2)
};
```

---

## 🚨 Transient Errors

The following HTTP status codes are considered transient errors and will trigger the retry mechanism:

- 408 (Request Timeout)
- 429 (Too Many Requests)
- 500 (Internal Server Error)
- 502 (Bad Gateway)
- 503 (Service Unavailable)
- 504 (Gateway Timeout)

Additionally, network errors (`HttpRequestException`) and timeouts (`TaskCanceledException` when not user-initiated) are also treated as transient errors.

---

## 👩‍💻 Development

### 🧪 Running Tests

The project includes comprehensive unit tests using xUnit and Moq. To run the tests:

```
dotnet test ResilientHttpClient/ResilientHttpClient.Tests/ResilientHttpClient.Tests.csproj
```

The tests cover:
- Basic functionality
- Retry policy behavior
- Circuit breaker pattern
- Error handling
- Factory methods
- String content retrieval

---

## 🤔 Why Not Polly?

[Polly](https://github.com/App-vNext/Polly) is an excellent and mature resilience library for .NET, and it's the go-to choice for most .NET applications. **So why did I build this?**

### Unity Compatibility is Hard

While Polly is fantastic for traditional .NET applications, it presents several challenges for **Unity developers**:

1. **Dependency Hell** 🔗
   - Polly requires multiple external dependencies (especially for newer versions)
   - Unity's package management can struggle with complex dependency trees
   - Version conflicts between Polly's dependencies and Unity's built-in libraries are common

2. **Targeting Issues** 🎯
   - Polly v8 targets .NET Standard 2.0, .NET Framework 4.6.2+, and .NET 6+
   - While compatible, the multiple dependencies can cause conflicts in Unity's environment
   - IL2CPP compilation in Unity may have issues with complex dependency chains

3. **Complexity Overhead** 📚
   - Polly is incredibly powerful but comes with a learning curve
   - For basic retry + circuit breaker patterns, it can feel like overkill
   - Game developers often just need simple, reliable HTTP resilience without the enterprise feature set

4. **Size Matters** 📦
   - Mobile games need to minimize app size
   - Polly + dependencies add significant bloat
   - This library is a single DLL with **zero external dependencies**

### When to Use Each

**Use Polly when:**
- Building ASP.NET Core, console apps, or traditional .NET services
- You need advanced features (bulkhead isolation, hedging, reactive policies)
- You're working in an environment with good dependency management
- You want the most battle-tested, community-supported solution

**Use ResilientHttpClient when:**
- Building Unity games (mobile, desktop, or WebGL)
- You need simple retry + circuit breaker patterns that "just work"
- You want zero dependencies and minimal footprint
- You want to avoid NuGet dependency conflicts in Unity
- You value simplicity and ease of integration over advanced features

### The Bottom Line

**Polly is the better choice for most .NET applications.** But if you're building a Unity game and you've ever wrestled with NuGet packages, dependency conflicts, or IL2CPP build errors, this library was built for you. It's focused, lightweight, and designed to work seamlessly in Unity's ecosystem.

---

## 🤝 Contributing

Contributions, issues and feature requests are welcome! Feel free to check [issues page](../../issues) or submit a pull request. Let's make .NET HTTP resilient for everyone! 💪

---

## 📄 License

MIT
