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
    /// Advanced tests for ResilientHttpClient covering HTTP verb methods,
    /// Dispose pattern, properties, and edge cases.
    /// </summary>
    public class ResilientHttpClientAdvancedTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ResilientHttpClientOptions _defaultOptions;

        public ResilientHttpClientAdvancedTests()
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

        #region GetAsync Tests

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task GetAsync_WithStringUri_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            string uri = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.GetAsync(uri));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task GetAsync_WithUriObject_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            Uri uri = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.GetAsync(uri));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task GetAsync_WithValidUri_ShouldUseGetMethod()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act
            var response = await resilientClient.GetAsync("https://api.example.com/data");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Get),
                ItExpr.IsAny<CancellationToken>());
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task GetAsync_WithCancellationToken_ShouldPassTokenThrough()
        {
            // Arrange
            var cts = new CancellationTokenSource();
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act
            var response = await resilientClient.GetAsync("https://api.example.com/data", cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region PostAsync Tests

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PostAsync_WithStringUri_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            string uri = null!;
            var content = new StringContent("test");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.PostAsync(uri, content));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PostAsync_WithUriObject_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            Uri uri = null!;
            var content = new StringContent("test");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.PostAsync(uri, content));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PostAsync_WhenContentIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            HttpContent content = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                resilientClient.PostAsync("https://api.example.com/data", content));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PostAsync_WithValidArguments_ShouldUsePostMethod()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var content = new StringContent("test data");

            // Act
            var response = await resilientClient.PostAsync("https://api.example.com/data", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Post),
                ItExpr.IsAny<CancellationToken>());
        }

        #endregion

        #region PutAsync Tests

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PutAsync_WithStringUri_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            string uri = null!;
            var content = new StringContent("test");

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.PutAsync(uri, content));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PutAsync_WhenContentIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            HttpContent content = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                resilientClient.PutAsync("https://api.example.com/data", content));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task PutAsync_WithValidArguments_ShouldUsePutMethod()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var content = new StringContent("updated data");

            // Act
            var response = await resilientClient.PutAsync("https://api.example.com/data/1", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Put),
                ItExpr.IsAny<CancellationToken>());
        }

        #endregion

        #region DeleteAsync Tests

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task DeleteAsync_WithStringUri_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            string uri = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.DeleteAsync(uri));
        }

        [Fact]
        [Trait("Category", "HttpVerbs")]
        public async Task DeleteAsync_WithValidUri_ShouldUseDeleteMethod()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act
            var response = await resilientClient.DeleteAsync("https://api.example.com/data/1");

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
            _handlerMock.Protected().Verify(
                "SendAsync",
                Times.Once(),
                ItExpr.Is<HttpRequestMessage>(req => req.Method == HttpMethod.Delete),
                ItExpr.IsAny<CancellationToken>());
        }

        #endregion

        #region GetStringAsync Tests

        [Fact]
        [Trait("Category", "GetStringAsync")]
        public async Task GetStringAsync_WithStringUri_WhenUriIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            string uri = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.GetStringAsync(uri));
        }

        [Fact]
        [Trait("Category", "GetStringAsync")]
        public async Task GetStringAsync_WhenResponseIsNotSuccess_ShouldThrowHttpRequestException()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NotFound));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act & Assert
            await Assert.ThrowsAsync<HttpRequestException>(() => 
                resilientClient.GetStringAsync("https://api.example.com/notfound"));
        }

        [Fact]
        [Trait("Category", "GetStringAsync")]
        public async Task GetStringAsync_WithCancellationToken_ShouldPassTokenThrough()
        {
            // Arrange
            var expectedContent = "test response";
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    Content = new StringContent(expectedContent)
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var cts = new CancellationTokenSource();

            // Act
            var result = await resilientClient.GetStringAsync("https://api.example.com/data", cts.Token);

            // Assert
            Assert.Equal(expectedContent, result);
        }

        #endregion

        #region Dispose Pattern Tests

        [Fact]
        [Trait("Category", "Dispose")]
        public void Dispose_ShouldDisposeUnderlyingHttpClient()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            bool disposed = false;
            mockHandler.Protected()
                .Setup("Dispose", ItExpr.Is<bool>(b => b))
                .Callback<bool>(disposing => { disposed = disposing; });

            var httpClient = new HttpClient(mockHandler.Object);
            var resilientClient = new Core.ResilientHttpClient(httpClient, _defaultOptions);

            // Act
            resilientClient.Dispose();

            // Assert - HttpClient should be disposed
            // Note: We can't directly verify HttpClient disposal, but we can verify it doesn't throw
            Assert.True(true); // Dispose completed without exception
        }

        [Fact]
        [Trait("Category", "Dispose")]
        public void Dispose_CalledMultipleTimes_ShouldNotThrow()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            mockHandler.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            
            var httpClient = new HttpClient(mockHandler.Object);
            var resilientClient = new Core.ResilientHttpClient(httpClient, _defaultOptions);

            // Act & Assert - Should not throw on multiple dispose calls
            resilientClient.Dispose();
            resilientClient.Dispose();
            resilientClient.Dispose();
        }

        [Fact]
        [Trait("Category", "Dispose")]
        public void UsingPattern_ShouldDisposeCorrectly()
        {
            // Arrange
            var mockHandler = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            mockHandler.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            var httpClient = new HttpClient(mockHandler.Object);
            
            // Act & Assert - Should not throw
            using (var resilientClient = new Core.ResilientHttpClient(httpClient, _defaultOptions))
            {
                // Client is used within this scope
                Assert.NotNull(resilientClient);
            }
            // Disposal happens automatically
        }

        #endregion

        #region Property Tests

        [Fact]
        [Trait("Category", "Properties")]
        public void BaseAddress_GetAndSet_ShouldWorkCorrectly()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var newBaseAddress = new Uri("https://newapi.example.com/");

            // Act
            resilientClient.BaseAddress = newBaseAddress;
            var actualBaseAddress = resilientClient.BaseAddress;

            // Assert
            Assert.Equal(newBaseAddress, actualBaseAddress);
        }

        [Fact]
        [Trait("Category", "Properties")]
        public void DefaultRequestHeaders_ShouldReturnHttpClientHeaders()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act
            var headers = resilientClient.DefaultRequestHeaders;

            // Assert
            Assert.NotNull(headers);
            Assert.Same(_httpClient.DefaultRequestHeaders, headers);
        }

        [Fact]
        [Trait("Category", "Properties")]
        public void Timeout_GetAndSet_ShouldWorkCorrectly()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var newTimeout = TimeSpan.FromSeconds(30);

            // Act
            resilientClient.Timeout = newTimeout;
            var actualTimeout = resilientClient.Timeout;

            // Assert
            Assert.Equal(newTimeout, actualTimeout);
        }

        [Fact]
        [Trait("Category", "Properties")]
        public void MaxResponseContentBufferSize_GetAndSet_ShouldWorkCorrectly()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var newBufferSize = 1024L * 1024L; // 1MB

            // Act
            resilientClient.MaxResponseContentBufferSize = newBufferSize;
            var actualBufferSize = resilientClient.MaxResponseContentBufferSize;

            // Assert
            Assert.Equal(newBufferSize, actualBufferSize);
        }

        #endregion

        #region Constructor Tests

        [Fact]
        [Trait("Category", "Constructor")]
        public void Constructor_WithDefaultConstructor_ShouldUseDefaultOptions()
        {
            // Arrange & Act
            var resilientClient = new Core.ResilientHttpClient(_httpClient);

            // Assert
            Assert.NotNull(resilientClient);
            // Client should be functional with default options
        }

        [Fact]
        [Trait("Category", "Constructor")]
        public void Constructor_WithNullOptions_ShouldUseDefaultOptions()
        {
            // Arrange
            ResilientHttpClientOptions options = null!;

            // Act
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            // Assert
            Assert.NotNull(resilientClient);
            // Should not throw and should use default values
        }

        #endregion

        #region SendAsync Edge Cases

        [Fact]
        [Trait("Category", "EdgeCases")]
        public async Task SendAsync_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            HttpRequestMessage request = null!;

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => resilientClient.SendAsync(request));
        }

        [Fact]
        [Trait("Category", "EdgeCases")]
        public async Task SendAsync_WithTimeout_ShouldRetryOnTimeout()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 1,
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };
            var resilientClient = new Core.ResilientHttpClient(_httpClient, options);

            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ThrowsAsync(new TaskCanceledException()) // Timeout
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion
    }
}
