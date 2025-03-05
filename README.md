# ResilientHttpClient

A drop-in replacement for HttpClient that adds common resiliency patterns such as circuit breaker, retry, and timeout. This library is compatible with .NET Standard 2.1 and can be used in Unity projects.

## Features

- **Circuit Breaker Pattern**: Prevents cascading failures by stopping requests after a certain number of failures.
- **Retry Pattern**: Automatically retries transient failures with configurable retry count and delay.
- **Timeout Handling**: Properly handles timeouts and treats them as transient failures.
- **Drop-in Replacement**: Implements the same interface as HttpClient, making it easy to replace existing code.
- **No External Dependencies**: Uses only native .NET Standard 2.1 features, making it compatible with Unity.
- **Well-Tested**: Comprehensive unit tests ensure reliability and correct behavior.
- **Complete API Coverage**: Supports all HttpClient methods including GetStringAsync for direct string responses.

## Installation

### For .NET Projects

1. Add the ResilientHttpClient.Core.dll to your project references.
2. Make sure you have a reference to System.Net.Http in your project.

### For Unity Projects

1. Place the ResilientHttpClient.Core.dll in your Assets/Plugins folder.
2. No additional dependencies are required.

## Building from Source

The project includes two batch files to simplify building:

- **build.bat**: Builds the project in Release mode and outputs the DLL location.
- **build-and-copy.bat**: Builds the project and optionally copies the DLL to your Unity project.

To build and copy to a Unity project:

```
build-and-copy.bat "C:\Path\To\Your\UnityProject"
```

## Usage

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

### Manual Creation

```csharp
// If you need more control, you can create the client manually
var httpClient = new HttpClient();
var resilientClient = new ResilientHttpClient(httpClient, options);
```

## Configuration Options

- **MaxRetries**: Maximum number of retry attempts for transient failures (default: 3)
- **RetryDelay**: Delay between retry attempts (default: 1 second)
- **MaxFailures**: Number of failures before the circuit breaker opens (default: 5)
- **CircuitResetTime**: Time to keep the circuit breaker open before allowing a trial request (default: 30 seconds)

## Transient Errors

The following HTTP status codes are considered transient errors and will trigger the retry mechanism:

- 408 (Request Timeout)
- 429 (Too Many Requests)
- 500 (Internal Server Error)
- 502 (Bad Gateway)
- 503 (Service Unavailable)
- 504 (Gateway Timeout)

Additionally, network errors (HttpRequestException) and timeouts (TaskCanceledException when not user-initiated) are also treated as transient errors.

## Development

### Running Tests

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

## License

MIT 