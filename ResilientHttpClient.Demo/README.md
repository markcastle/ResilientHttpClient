# ResilientHttpClient Demo

Simple console application demonstrating the key features of ResilientHttpClient.

## Running the Demo

```bash
cd ResilientHttpClient.Demo
dotnet run
```

Or from the solution root:
```bash
dotnet run --project ResilientHttpClient.Demo
```

## What It Demonstrates

### Example 1: Basic GET Request
- Creating a resilient client using the factory
- Making a simple HTTP request
- Clean, drop-in replacement for HttpClient

### Example 2: Retry Logic
- Configuring retry settings (MaxRetries, RetryDelay)
- Automatic retry on transient failures (5xx, timeouts, network errors)
- No code changes needed - retries happen automatically

### Example 3: Custom Policies
- Creating per-request policies using RequestPolicyBuilder
- Overriding default settings for specific requests
- Fluent API for easy configuration

### Example 4: Circuit Breaker
- Protection against cascading failures
- Automatic circuit opening after repeated failures
- Circuit closing after successful requests
- Immediate rejection when circuit is open

## Notes

- Uses JSONPlaceholder API (https://jsonplaceholder.typicode.com) for real HTTP calls
- Example 4 intentionally uses an invalid domain to demonstrate circuit breaker
- All examples include detailed comments explaining what's happening
- Press any key to exit after demo completes

## Learn More

See the main README.md for full documentation and advanced usage.
