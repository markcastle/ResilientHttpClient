# ResilientHttpClient 🚀

```
![Coverage](https://img.shields.io/badge/coverage-100%25-brightgreen?style=flat-square)
![License](https://img.shields.io/badge/License-MIT-blue.svg?style=flat-square)
![.NET Standard](https://img.shields.io/badge/.NET%20Standard-2.1-blueviolet?style=flat-square)
```

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

---

## ⚡ Quickstart

```csharp
var client = ResilientHttpClientFactory.CreateClient();
var response = await client.GetAsync("https://api.example.com/data");
```

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

## 🤝 Contributing

Contributions, issues and feature requests are welcome! Feel free to check [issues page](../../issues) or submit a pull request. Let’s make .NET HTTP resilient for everyone! 💪

---

## 📄 License

MIT
