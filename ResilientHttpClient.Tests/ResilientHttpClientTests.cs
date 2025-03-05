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
    /// Tests for the ResilientHttpClient class to verify its resilience patterns
    /// including retry policies, circuit breaker, and error handling.
    /// </summary>
    public class ResilientHttpClientTests : IDisposable
    {
        // Common test objects
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ResilientHttpClientOptions _defaultOptions;

        public ResilientHttpClientTests()
        {
            // Create a mock HttpMessageHandler with Loose behavior instead of Strict
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            
            // Setup the Dispose method to do nothing
            _handlerMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            
            // Create an HttpClient with the mock handler
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };

            // Default options for testing
            _defaultOptions = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromMilliseconds(10),
                MaxFailures = 5,
                CircuitResetTime = TimeSpan.FromSeconds(1)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region Constructor Tests

        [Fact]
        [Trait("Category", "BasicFunctionality")]
        public void Constructor_WhenHttpClientIsNull_ShouldThrowArgumentNullException()
        {
            // Act & Assert
            HttpClient nullClient = null!;
            var exception = Assert.Throws<ArgumentNullException>(() => new Core.ResilientHttpClient(nullClient));
            Assert.Equal("httpClient", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "BasicFunctionality")]
        public void CreateClient_WithCustomOptions_ShouldUseProvidedOptions()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };

            // Act
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            // Assert
            // We can't directly test the options values since they're private
            // Instead, we'll verify the client was created successfully
            Assert.NotNull(resilientClient);
        }

        #endregion

        #region Basic Functionality Tests

        [Fact]
        [Trait("Category", "BasicFunctionality")]
        public async Task SendAsync_WhenRequestIsSuccessful_ShouldReturnResponseWithoutRetry()
        {
            // Arrange
            var expectedResponse = new HttpResponseMessage(HttpStatusCode.OK);
            
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(expectedResponse);

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify SendAsync was called exactly once
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "RetryPolicy")]
        public async Task SendAsync_WhenTransientErrorOccurs_ShouldRetryUpToMaxRetries()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 2,
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify SendAsync was called exactly 3 times (initial + 2 retries)
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "RetryPolicy")]
        public async Task SendAsync_WhenNetworkErrorOccurs_ShouldRetryUpToMaxRetries()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 1,
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify SendAsync was called exactly 2 times (initial + 1 retry)
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "RetryPolicy")]
        public async Task SendAsync_WhenNonTransientErrorOccurs_ShouldNotRetry()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
            
            // Verify SendAsync was called exactly once
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        #endregion

        #region Circuit Breaker Tests

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task SendAsync_WhenMaxFailuresReached_ShouldOpenCircuit()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 2,
                MaxRetries = 0, // No retries for this test
                CircuitResetTime = TimeSpan.FromSeconds(1)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable));

            // Act - Send requests until circuit opens
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));

            // Assert - Next request should throw
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => 
                resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")));
            
            Assert.Contains("Circuit is open", exception.Message);
        }

        [Fact]
        [Trait("Category", "CircuitBreaker")]
        public async Task SendAsync_AfterSuccessfulRequest_ShouldResetFailureCount()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 3, // Increase to 3 to avoid opening the circuit too early
                MaxRetries = 0, // No retries for this test
                CircuitResetTime = TimeSpan.FromSeconds(1)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)) // First failure
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)) // Success resets count
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)) // First failure again
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)) // Second failure
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)); // Final request

            // Act - Send requests with a success in between
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")); // Failure 1
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")); // Success (resets count)
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")); // Failure 1
            await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test")); // Failure 2

            // Assert - Circuit should still be closed, so this should not throw
            var response = await resilientClient.SendAsync(new HttpRequestMessage(HttpMethod.Get, "/api/test"));
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region Transient Error Tests

        [Theory]
        [Trait("Category", "RetryPolicy")]
        [InlineData(HttpStatusCode.ServiceUnavailable)]
        [InlineData(HttpStatusCode.RequestTimeout)]
        [InlineData(HttpStatusCode.GatewayTimeout)]
        [InlineData(HttpStatusCode.TooManyRequests)]
        public async Task SendAsync_WithDifferentTransientErrors_ShouldRetry(HttpStatusCode statusCode)
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(statusCode))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            
            // Verify SendAsync was called exactly 2 times (initial + 1 retry)
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        #endregion

        #region Factory Tests

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithBaseAddress_ShouldSetBaseAddressCorrectly()
        {
            // Arrange
            string baseAddress = "https://api.example.com/";
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 10,
                RetryDelay = TimeSpan.FromSeconds(5),
                MaxFailures = 20,
                CircuitResetTime = TimeSpan.FromMinutes(5)
            };

            // Act
            var client = ResilientHttpClientFactory.CreateClient(baseAddress, options);

            // Assert
            Assert.NotNull(client);
            Assert.Equal(new Uri(baseAddress), client.BaseAddress);
        }

        #endregion
    }
} 