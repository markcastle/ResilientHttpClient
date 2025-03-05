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
    /// Tests for the per-request policy feature of ResilientHttpClient.
    /// </summary>
    public class RequestPolicyTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ResilientHttpClientOptions _defaultOptions;

        public RequestPolicyTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _handlerMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };

            _defaultOptions = new ResilientHttpClientOptions
            {
                MaxRetries = 1,
                RetryDelay = TimeSpan.FromMilliseconds(10),
                MaxFailures = 3,
                CircuitResetTime = TimeSpan.FromSeconds(1)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        [Fact]
        [Trait("Category", "RequestPolicy")]
        public async Task SendAsync_WithCustomRetryPolicy_ShouldUseCustomRetryCount()
        {
            // Arrange
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            
            // Create request with custom policy (3 retries instead of default 1)
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test")
                .WithPolicy(policy => policy.WithMaxRetries(3));

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify SendAsync was called exactly 4 times (initial + 3 retries)
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(4),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "RequestPolicy")]
        public async Task SendAsync_WithDisabledRetries_ShouldNotRetry()
        {
            // Arrange
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            
            // Create request with disabled retries
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test")
                .WithPolicy(policy => policy.DisableRetries());

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => resilientClient.SendAsync(request));
            
            // Verify SendAsync was called exactly once (no retries)
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "RequestPolicy")]
        public async Task SendAsync_WithCustomRetryDelay_ShouldUseCustomDelay()
        {
            // Arrange
            var startTime = DateTime.UtcNow;
            var customDelay = TimeSpan.FromMilliseconds(500); // Longer delay for easier testing
            
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            
            // Create request with custom retry delay
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test")
                .WithPolicy(policy => policy.WithRetryDelay(customDelay));

            // Act
            var response = await resilientClient.SendAsync(request);
            var elapsedTime = DateTime.UtcNow - startTime;

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify the delay was at least the custom delay (with some tolerance)
            Assert.True(elapsedTime >= customDelay.Subtract(TimeSpan.FromMilliseconds(50)));
            
            // Verify SendAsync was called exactly 2 times (initial + 1 retry)
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "RequestPolicy")]
        public async Task SendAsync_WithBypassCircuitBreaker_ShouldBypassOpenCircuit()
        {
            // Arrange - Set up a scenario where the circuit would be open
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            
            // Trip the circuit breaker with multiple failures
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            for (int i = 0; i < _defaultOptions.MaxFailures; i++)
            {
                var failRequest = new HttpRequestMessage(HttpMethod.Get, "/api/test");
                
                try
                {
                    await resilientClient.SendAsync(failRequest);
                }
                catch
                {
                    // Ignore exceptions, we're just trying to trip the circuit breaker
                }
            }

            // Now the circuit should be open
            
            // Set up a successful response for our bypass request
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));
            
            // First verify the circuit is open with a regular request
            var testRequest = new HttpRequestMessage(HttpMethod.Get, "/api/test");
            await Assert.ThrowsAsync<HttpRequestException>(() => resilientClient.SendAsync(testRequest));
            
            // Now try with a request that bypasses the circuit breaker
            var bypassRequest = new HttpRequestMessage(HttpMethod.Get, "/api/test")
                .WithPolicy(policy => policy.BypassCircuitBreaker());

            // Act
            var response = await resilientClient.SendAsync(bypassRequest);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }
    }
} 