using System;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using ResilientHttpClient.Core;
using Xunit;
using Moq;
using Moq.Protected;

namespace ResilientHttpClient.Tests
{
    /// <summary>
    /// Advanced circuit breaker tests covering half-open state and state transitions.
    /// </summary>
    public class CircuitBreakerAdvancedTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;

        public CircuitBreakerAdvancedTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _handlerMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task CircuitBreaker_AfterResetTime_ShouldAllowTestRequest()
        {
            // Arrange - Circuit will open quickly and reset quickly
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 2,
                MaxRetries = 0,
                CircuitResetTime = TimeSpan.FromMilliseconds(100) // Short reset time for testing
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            // Setup failures to open circuit
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            // Act - Trip the circuit breaker
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));

            // Circuit should now be open
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")));

            // Wait for circuit reset time to elapse
            await Task.Delay(150);

            // Setup a successful response for the test request (half-open state)
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Assert - Circuit should allow a test request (half-open)
            var response = await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task CircuitBreaker_HalfOpenSuccess_ShouldCloseCircuit()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 2,
                MaxRetries = 0,
                CircuitResetTime = TimeSpan.FromMilliseconds(100)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            // Setup failures to open circuit
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // Half-open success
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // Circuit closed

            // Act - Trip the circuit
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));

            // Wait for reset
            await Task.Delay(150);

            // Make successful request (half-open -> closed)
            var response1 = await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            Assert.Equal(HttpStatusCode.OK, response1.StatusCode);

            // Circuit should now be closed - another request should work immediately
            var response2 = await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            Assert.Equal(HttpStatusCode.OK, response2.StatusCode);
        }

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task CircuitBreaker_RetryExhaustion_ShouldIncrementFailureCount()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 2,
                MaxRetries = 2,
                RetryDelay = TimeSpan.FromMilliseconds(10),
                CircuitResetTime = TimeSpan.FromSeconds(10)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            // Setup continuous failures
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            // Act - Exhaust retries twice to reach MaxFailures
            var response1 = await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response1.StatusCode);

            var response2 = await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            Assert.Equal(HttpStatusCode.ServiceUnavailable, response2.StatusCode);

            // Assert - Circuit should now be open
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")));
        }

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task SendAsync_WithHttpCompletionOption_ShouldWorkCorrectly()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 1,
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            var response = await resilientClient.SendAsync(request, HttpCompletionOption.ResponseContentRead);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task SendAsync_WithHttpCompletionOption_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, new ResilientHttpClientOptions());
            HttpRequestMessage request = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                resilientClient.SendAsync(request, HttpCompletionOption.ResponseContentRead));
        }
    }
}
