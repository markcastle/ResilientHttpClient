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
    public class ResilientHttpClientTests
    {
        [Fact]
        public async Task SendAsync_SuccessfulRequest_ReturnsResponse()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.OK,
                Content = new StringContent("test content")
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var resilientClient = new Core.ResilientHttpClient(httpClient);

            // Act
            var result = await resilientClient.GetAsync("http://test.com");

            // Assert
            Assert.Equal(HttpStatusCode.OK, result.StatusCode);
            var content = await result.Content.ReadAsStringAsync();
            Assert.Equal("test content", content);

            // Verify the request was sent exactly once
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_TransientError_RetriesToMaxRetries()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.ServiceUnavailable
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromMilliseconds(10) // Short delay for tests
            };
            var resilientClient = new Core.ResilientHttpClient(httpClient, options);

            // Act
            var result = await resilientClient.GetAsync("http://test.com");

            // Assert
            Assert.Equal(HttpStatusCode.ServiceUnavailable, result.StatusCode);

            // Verify the request was sent exactly 4 times (1 original + 3 retries)
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(4),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_NonTransientError_DoesNotRetry()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            var response = new HttpResponseMessage
            {
                StatusCode = HttpStatusCode.NotFound
            };

            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(response);

            var httpClient = new HttpClient(handlerMock.Object);
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromMilliseconds(10) // Short delay for tests
            };
            var resilientClient = new Core.ResilientHttpClient(httpClient, options);

            // Act
            var result = await resilientClient.GetAsync("http://test.com");

            // Assert
            Assert.Equal(HttpStatusCode.NotFound, result.StatusCode);

            // Verify the request was sent exactly once (no retries)
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(1),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_NetworkError_RetriesToMaxRetries()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 2,
                RetryDelay = TimeSpan.FromMilliseconds(10) // Short delay for tests
            };
            var resilientClient = new Core.ResilientHttpClient(httpClient, options);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => resilientClient.GetAsync("http://test.com"));

            // Verify the request was sent exactly 3 times (1 original + 2 retries)
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(3),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public async Task SendAsync_CircuitBreaker_OpensAfterMaxFailures()
        {
            // Arrange
            var handlerMock = new Mock<HttpMessageHandler>();
            
            handlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new HttpRequestException("Network error"));

            var httpClient = new HttpClient(handlerMock.Object);
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 2,
                MaxRetries = 0, // No retries for this test
                CircuitResetTime = TimeSpan.FromSeconds(30)
            };
            var resilientClient = new Core.ResilientHttpClient(httpClient, options);

            // Act - First request (failure 1)
            await Assert.ThrowsAsync<HttpRequestException>(() => resilientClient.GetAsync("http://test.com"));
            
            // Act - Second request (failure 2)
            await Assert.ThrowsAsync<HttpRequestException>(() => resilientClient.GetAsync("http://test.com"));
            
            // Act - Third request (circuit should be open)
            var exception = await Assert.ThrowsAsync<HttpRequestException>(() => resilientClient.GetAsync("http://test.com"));
            
            // Assert
            Assert.Contains("Circuit is open", exception.Message);
            
            // Verify the request was sent exactly 2 times (circuit opens after 2 failures)
            handlerMock.Protected().Verify(
                "SendAsync",
                Times.Exactly(2),
                ItExpr.IsAny<HttpRequestMessage>(),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        public void Factory_CreateClient_ReturnsResilientHttpClient()
        {
            // Act
            var client = ResilientHttpClientFactory.CreateClient();

            // Assert
            Assert.NotNull(client);
            Assert.IsAssignableFrom<IResilientHttpClient>(client);
        }

        [Fact]
        public void Factory_CreateClientWithBaseAddress_SetsBaseAddress()
        {
            // Arrange
            string baseAddress = "http://test.com/";

            // Act
            var client = ResilientHttpClientFactory.CreateClient(baseAddress);

            // Assert
            Assert.NotNull(client);
            Assert.Equal(new Uri(baseAddress), client.BaseAddress);
        }
    }
} 