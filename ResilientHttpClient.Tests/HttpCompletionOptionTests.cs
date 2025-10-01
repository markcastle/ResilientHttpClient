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
    /// Tests for HTTP verb methods with HttpCompletionOption parameters
    /// to achieve 90%+ code coverage.
    /// </summary>
    public class HttpCompletionOptionTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ResilientHttpClientOptions _defaultOptions;

        public HttpCompletionOptionTests()
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
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region GetAsync with HttpCompletionOption

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task GetAsync_StringUri_WithResponseHeadersRead_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act
            var response = await resilientClient.GetAsync("/api/test", HttpCompletionOption.ResponseHeadersRead);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task GetAsync_StringUri_WithResponseContentRead_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);

            // Act
            var response = await resilientClient.GetAsync("/api/test", HttpCompletionOption.ResponseContentRead);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task GetAsync_UriObject_WithHttpCompletionOption_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var uri = new Uri("https://api.example.com/data");

            // Act
            var response = await resilientClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task GetAsync_StringUri_WithHttpCompletionOptionAndCancellationToken_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var cts = new CancellationTokenSource();

            // Act
            var response = await resilientClient.GetAsync("/api/test", HttpCompletionOption.ResponseContentRead, cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task GetAsync_UriObject_WithHttpCompletionOptionAndCancellationToken_WhenUriIsNull_ShouldThrow()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            Uri uri = null!;
            var cts = new CancellationTokenSource();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                resilientClient.GetAsync(uri, HttpCompletionOption.ResponseContentRead, cts.Token));
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task GetAsync_UriObject_WithHttpCompletionOptionAndCancellationToken_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var uri = new Uri("https://api.example.com/data");
            var cts = new CancellationTokenSource();

            // Act
            var response = await resilientClient.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion

        #region PostAsync with CancellationToken

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task PostAsync_StringUri_WithCancellationToken_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.Created));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var content = new StringContent("test data");
            var cts = new CancellationTokenSource();

            // Act
            var response = await resilientClient.PostAsync("/api/test", content, cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        }

        #endregion

        #region PutAsync with CancellationToken

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task PutAsync_StringUri_WithCancellationToken_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var content = new StringContent("updated data");
            var cts = new CancellationTokenSource();

            // Act
            var response = await resilientClient.PutAsync("/api/test/1", content, cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task PutAsync_UriObject_WithCancellationToken_WhenUriIsNull_ShouldThrow()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            Uri uri = null!;
            var content = new StringContent("test");
            var cts = new CancellationTokenSource();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                resilientClient.PutAsync(uri, content, cts.Token));
        }

        #endregion

        #region DeleteAsync with CancellationToken

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task DeleteAsync_StringUri_WithCancellationToken_ShouldSucceed()
        {
            // Arrange
            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.NoContent));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var cts = new CancellationTokenSource();

            // Act
            var response = await resilientClient.DeleteAsync("/api/test/1", cts.Token);

            // Assert
            Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
        }

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task DeleteAsync_UriObject_WithCancellationToken_WhenUriIsNull_ShouldThrow()
        {
            // Arrange
            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            Uri uri = null!;
            var cts = new CancellationTokenSource();

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => 
                resilientClient.DeleteAsync(uri, cts.Token));
        }

        #endregion

        #region Retry with HttpCompletionOption

        [Fact]
        [Trait("Category", "HttpCompletionOption")]
        public async Task SendAsync_WithHttpCompletionOption_WhenRetrySucceeds_ShouldReturnSuccess()
        {
            // Arrange
            _handlerMock.Protected()
                .SetupSequence<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.ServiceUnavailable))
                .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _defaultOptions);
            var request = new HttpRequestMessage(HttpMethod.Get, "/api/test");

            // Act
            var response = await resilientClient.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        }

        #endregion
    }
}
