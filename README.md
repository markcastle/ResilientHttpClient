# ResilientHttpClient

A drop-in replacement for HttpClient that adds common resiliency patterns such as circuit breaker, retry, and timeout. This library is compatible with .NET Standard 2.1 and can be used in Unity projects.

## Features

- **Circuit Breaker Pattern**: Prevents cascading failures by stopping requests after a certain number of failures.
- **Retry Pattern**: Automatically retries transient failures with configurable retry count and delay.
- **Timeout Handling**: Properly handles timeouts and treats them as transient failures.
- **Drop-in Replacement**: Implements the same interface as HttpClient, making it easy to replace existing code.
- **No External Dependencies**: Uses only native .NET Standard 2.1 features, making it compatible with Unity.

## Installation

1. Add the ResilientHttpClient.Core.dll to your project references.
2. For Unity, place the DLL in your Assets/Plugins folder.

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

## License

MIT 