using System;
using System.Net.Http;
using System.Threading.Tasks;
using ResilientHttpClient.Core;

namespace ResilientHttpClient.Demo
{
    /// <summary>
    /// Simple demo showing the key features of ResilientHttpClient.
    /// Uses JSONPlaceholder (https://jsonplaceholder.typicode.com) - a free fake API for testing.
    /// </summary>
    class Program
    {
        static async Task Main(string[] args)
        {
            Console.WriteLine("=== ResilientHttpClient Demo ===\n");

            // Run all examples
            await Example1_BasicGetRequest();
            await Example2_RetryInAction();
            await Example3_CustomPolicy();
            await Example4_CircuitBreaker();

            Console.WriteLine("\n=== Demo Complete ===");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        /// <summary>
        /// Example 1: Basic GET request using the factory
        /// Shows how simple it is to create and use the client
        /// </summary>
        static async Task Example1_BasicGetRequest()
        {
            Console.WriteLine("\n--- Example 1: Basic GET Request ---");

            // Create a resilient client using the factory
            var client = ResilientHttpClientFactory.CreateClient("https://jsonplaceholder.typicode.com");

            try
            {
                // Make a simple GET request
                var response = await client.GetAsync("/posts/1");
                var content = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine($"Content preview: {content.Substring(0, Math.Min(100, content.Length))}...");
                Console.WriteLine("✓ Request succeeded!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Example 2: Show retry logic in action
        /// Demonstrates automatic retry on failures with configurable settings
        /// </summary>
        static async Task Example2_RetryInAction()
        {
            Console.WriteLine("\n--- Example 2: Retry Logic ---");

            // Create client with custom retry settings
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromMilliseconds(500)
            };

            var client = ResilientHttpClientFactory.CreateClient("https://jsonplaceholder.typicode.com", options);

            try
            {
                Console.WriteLine("Making request with retry enabled (MaxRetries=3)...");
                
                // This endpoint exists and should succeed immediately
                var response = await client.GetAsync("/posts/1");
                
                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine("✓ Request succeeded (no retries needed)");
                
                // Note: To see retries in action, you would need an endpoint that 
                // returns 5xx errors or times out. The library automatically retries
                // on transient failures.
                Console.WriteLine("\nℹ Retry happens automatically on:");
                Console.WriteLine("  - 5xx server errors (500, 502, 503, 504)");
                Console.WriteLine("  - 408 Request Timeout");
                Console.WriteLine("  - Network failures (HttpRequestException)");
                Console.WriteLine("  - Timeouts (TaskCanceledException)");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Request failed after retries: {ex.Message}");
            }
        }

        /// <summary>
        /// Example 3: Using custom per-request policies
        /// Shows how to override default settings for specific requests
        /// </summary>
        static async Task Example3_CustomPolicy()
        {
            Console.WriteLine("\n--- Example 3: Custom Policy ---");

            // Create client with default settings
            var client = ResilientHttpClientFactory.CreateClient("https://jsonplaceholder.typicode.com");

            // Create a custom policy for this specific request
            var customPolicy = new RequestPolicyBuilder()
                .WithMaxRetries(5)                              // More retries than default
                .WithRetryDelay(TimeSpan.FromMilliseconds(200)) // Faster retry
                .Build();

            try
            {
                // Create a request and attach the custom policy
                var request = new HttpRequestMessage(HttpMethod.Get, "/posts/1");
                request.WithPolicy(customPolicy);

                Console.WriteLine("Making request with custom policy (MaxRetries=5, Delay=200ms)...");
                var response = await client.SendAsync(request);

                Console.WriteLine($"Status: {response.StatusCode}");
                Console.WriteLine("✓ Request succeeded with custom policy!");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"✗ Request failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Example 4: Circuit breaker demonstration
        /// Shows how the circuit breaker protects against cascading failures
        /// </summary>
        static async Task Example4_CircuitBreaker()
        {
            Console.WriteLine("\n--- Example 4: Circuit Breaker ---");

            // Create client with aggressive circuit breaker settings for demo
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 3,                        // Open circuit after 3 failures
                CircuitResetTime = TimeSpan.FromSeconds(5), // Try again after 5 seconds
                MaxRetries = 0                          // Disable retries for cleaner demo
            };

            var client = ResilientHttpClientFactory.CreateClient("https://invalid-domain-that-does-not-exist-12345.com", options);

            Console.WriteLine("Circuit breaker settings:");
            Console.WriteLine("  - MaxFailures: 3");
            Console.WriteLine("  - CircuitResetTime: 5 seconds");
            Console.WriteLine("\nSimulating failures to trip the circuit breaker...\n");

            // Make several requests that will fail (invalid domain)
            for (int i = 1; i <= 5; i++)
            {
                try
                {
                    Console.Write($"Attempt {i}: ");
                    var response = await client.GetAsync("/api/test");
                    Console.WriteLine($"Success - {response.StatusCode}");
                }
                catch (HttpRequestException ex) when (ex.Message.Contains("Circuit is open"))
                {
                    // Circuit breaker is open - requests are rejected immediately
                    Console.WriteLine("✗ CIRCUIT OPEN - Request blocked!");
                    Console.WriteLine("   (Circuit breaker is protecting against further failures)");
                }
                catch (Exception ex)
                {
                    // Regular failure (before circuit opens)
                    Console.WriteLine($"✗ Failed - {ex.GetType().Name}");
                }

                await Task.Delay(100); // Small delay between requests
            }

            Console.WriteLine("\nℹ Circuit breaker protects your application by:");
            Console.WriteLine("  - Detecting repeated failures");
            Console.WriteLine("  - Opening the circuit to prevent cascading failures");
            Console.WriteLine("  - Automatically trying again after reset time");
            Console.WriteLine("  - Closing the circuit when requests succeed again");
        }
    }
}
