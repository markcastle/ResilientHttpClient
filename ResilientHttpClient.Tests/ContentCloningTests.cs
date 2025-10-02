using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using ResilientHttpClient.Core;
using Xunit;
using Moq;
using Moq.Protected;

namespace ResilientHttpClient.Tests
{
    /// <summary>
    /// Tests for request content cloning during retries.
    /// CRITICAL: Ensures POST/PUT requests can be safely retried without content consumption issues.
    /// </summary>
    public class ContentCloningTests : IDisposable
    {
        private readonly Mock<HttpMessageHandler> _handlerMock;
        private readonly HttpClient _httpClient;
        private readonly ResilientHttpClientOptions _options;

        public ContentCloningTests()
        {
            _handlerMock = new Mock<HttpMessageHandler>(MockBehavior.Loose);
            _handlerMock.Protected().Setup("Dispose", ItExpr.IsAny<bool>());
            
            _httpClient = new HttpClient(_handlerMock.Object)
            {
                BaseAddress = new Uri("https://api.example.com/")
            };

            _options = new ResilientHttpClientOptions
            {
                MaxRetries = 2,
                RetryDelay = TimeSpan.FromMilliseconds(10)
            };
        }

        public void Dispose()
        {
            _httpClient?.Dispose();
        }

        #region POST Request Content Cloning

        [Fact]
        [Trait("Category", "ContentCloning")]
        [Trait("Priority", "Critical")]
        public async Task PostAsync_WithStringContent_WhenRetried_ShouldCloneContent()
        {
            // Arrange
            var expectedContent = "test data";
            int callCount = 0;
            string capturedContent1 = null!;
            string capturedContent2 = null!;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    // Capture the content from each attempt
                    var content = req.Content?.ReadAsStringAsync().Result;
                    if (callCount == 1)
                        capturedContent1 = content;
                    else if (callCount == 2)
                        capturedContent2 = content;

                    // First attempt fails, second succeeds
                    return callCount == 1
                        ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                        : new HttpResponseMessage(HttpStatusCode.Created);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var content = new StringContent(expectedContent);

            // Act
            var response = await resilientClient.PostAsync("/api/test", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(2, callCount); // Should have retried once
            Assert.Equal(expectedContent, capturedContent1); // First attempt received content
            Assert.Equal(expectedContent, capturedContent2); // Second attempt received CLONED content
        }

        [Fact]
        [Trait("Category", "ContentCloning")]
        [Trait("Priority", "Critical")]
        public async Task PostAsync_WithJsonContent_WhenRetried_ShouldPreserveContentAndHeaders()
        {
            // Arrange
            var jsonContent = "{\"name\":\"test\",\"value\":123}";
            int callCount = 0;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    // Verify content is present and readable
                    var receivedContent = req.Content?.ReadAsStringAsync().Result;
                    Assert.Equal(jsonContent, receivedContent);
                    
                    // Verify content-type header is preserved
                    Assert.Equal("application/json", req.Content?.Headers.ContentType?.MediaType);

                    return callCount == 1
                        ? new HttpResponseMessage(HttpStatusCode.InternalServerError)
                        : new HttpResponseMessage(HttpStatusCode.OK);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

            // Act
            var response = await resilientClient.PostAsync("/api/data", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, callCount);
        }

        #endregion

        #region PUT Request Content Cloning

        [Fact]
        [Trait("Category", "ContentCloning")]
        [Trait("Priority", "Critical")]
        public async Task PutAsync_WithByteArrayContent_WhenRetried_ShouldCloneContent()
        {
            // Arrange
            var originalBytes = Encoding.UTF8.GetBytes("binary data");
            int callCount = 0;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    // Verify content can be read multiple times (cloned)
                    var receivedBytes = req.Content?.ReadAsByteArrayAsync().Result;
                    Assert.True(originalBytes.SequenceEqual(receivedBytes));

                    return callCount == 1
                        ? new HttpResponseMessage(HttpStatusCode.RequestTimeout)
                        : new HttpResponseMessage(HttpStatusCode.OK);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var content = new ByteArrayContent(originalBytes);

            // Act
            var response = await resilientClient.PutAsync("/api/test/1", content);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, callCount);
        }

        [Fact]
        [Trait("Category", "ContentCloning")]
        [Trait("Priority", "Critical")]
        public async Task PutAsync_WithContentHeaders_WhenRetried_ShouldPreserveHeaders()
        {
            // Arrange
            int callCount = 0;
            var testData = "content with custom headers";

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    // Verify all content headers are preserved
                    Assert.Equal("application/custom", req.Content?.Headers.ContentType?.MediaType);
                    Assert.True(req.Content?.Headers.Contains("X-Custom-Header"));
                    Assert.Equal("custom-value", req.Content?.Headers.GetValues("X-Custom-Header").FirstOrDefault());

                    return callCount == 1
                        ? new HttpResponseMessage(HttpStatusCode.BadGateway)
                        : new HttpResponseMessage(HttpStatusCode.Accepted);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var content = new StringContent(testData, Encoding.UTF8, "application/custom");
            content.Headers.Add("X-Custom-Header", "custom-value");

            // Act
            var response = await resilientClient.PutAsync("/api/resource", content);

            // Assert
            Assert.Equal(HttpStatusCode.Accepted, response.StatusCode);
            Assert.Equal(2, callCount);
        }

        #endregion

        #region GET Request Without Content

        [Fact]
        [Trait("Category", "ContentCloning")]
        public async Task GetAsync_WithoutContent_WhenRetried_ShouldNotBreak()
        {
            // Arrange
            int callCount = 0;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    // GET requests should not have content
                    Assert.Null(req.Content);

                    return callCount == 1
                        ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                        : new HttpResponseMessage(HttpStatusCode.OK);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);

            // Act
            var response = await resilientClient.GetAsync("/api/data");

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(2, callCount);
        }

        #endregion

        #region SendAsync with Manual Request Creation

        [Fact]
        [Trait("Category", "ContentCloning")]
        [Trait("Priority", "Critical")]
        public async Task SendAsync_WithManualRequestAndContent_WhenRetried_ShouldCloneCorrectly()
        {
            // Arrange
            var testData = "manually created request content";
            int callCount = 0;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    var receivedContent = req.Content?.ReadAsStringAsync().Result;
                    Assert.Equal(testData, receivedContent);
                    Assert.Equal(HttpMethod.Post, req.Method);

                    return callCount == 1
                        ? throw new HttpRequestException("Network error")
                        : new HttpResponseMessage(HttpStatusCode.Created);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/manual")
            {
                Content = new StringContent(testData)
            };

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(2, callCount);
        }

        [Fact]
        [Trait("Category", "ContentCloning")]
        [Trait("Priority", "Critical")]
        public async Task SendAsync_WithMultipleRetries_ShouldCloneContentEachTime()
        {
            // Arrange
            var testData = "multi-retry content";
            int callCount = 0;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    // Each retry should receive valid content
                    var receivedContent = req.Content?.ReadAsStringAsync().Result;
                    Assert.Equal(testData, receivedContent);

                    // Fail twice, then succeed
                    if (callCount <= 2)
                        throw new HttpRequestException("Network error");
                    
                    return new HttpResponseMessage(HttpStatusCode.OK);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var request = new HttpRequestMessage(HttpMethod.Post, "/api/test")
            {
                Content = new StringContent(testData)
            };

            // Act
            var response = await resilientClient.SendAsync(request);

            // Assert
            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            Assert.Equal(3, callCount); // Original + 2 retries
        }

        #endregion

        #region Edge Cases

        [Fact]
        [Trait("Category", "ContentCloning")]
        public async Task PostAsync_WithEmptyContent_WhenRetried_ShouldHandleCorrectly()
        {
            // Arrange
            int callCount = 0;

            _handlerMock.Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>())
                .ReturnsAsync((HttpRequestMessage req, CancellationToken ct) =>
                {
                    callCount++;
                    
                    // Empty content should still be cloneable
                    var content = req.Content?.ReadAsStringAsync().Result;
                    Assert.Equal("", content);

                    return callCount == 1
                        ? new HttpResponseMessage(HttpStatusCode.ServiceUnavailable)
                        : new HttpResponseMessage(HttpStatusCode.Created);
                });

            var resilientClient = new Core.ResilientHttpClient(_httpClient, _options);
            var content = new StringContent("");

            // Act
            var response = await resilientClient.PostAsync("/api/empty", content);

            // Assert
            Assert.Equal(HttpStatusCode.Created, response.StatusCode);
            Assert.Equal(2, callCount);
        }

        #endregion
    }
}
