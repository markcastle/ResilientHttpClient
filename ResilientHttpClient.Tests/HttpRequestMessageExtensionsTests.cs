using System;
using System.Net.Http;
using ResilientHttpClient.Core;
using Xunit;

namespace ResilientHttpClient.Tests
{
    /// <summary>
    /// Tests for HttpRequestMessageExtensions to ensure all extension methods
    /// handle edge cases correctly and achieve 100% coverage.
    /// </summary>
    public class HttpRequestMessageExtensionsTests
    {
        #region WithPolicy(RequestPolicy) Tests

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void WithPolicy_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpRequestMessage request = null!;
            var policy = new RequestPolicy();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.WithPolicy(policy));
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void WithPolicy_WhenPolicyIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            RequestPolicy policy = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.WithPolicy(policy));
            Assert.Equal("policy", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void WithPolicy_WhenValidArguments_ShouldAttachPolicyToRequest()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            var policy = new RequestPolicy { MaxRetries = 5 };

            // Act
            var result = request.WithPolicy(policy);

            // Assert
            Assert.Same(request, result); // Should return same instance for fluent chaining
            var attachedPolicy = request.GetPolicy();
            Assert.NotNull(attachedPolicy);
            Assert.Equal(5, attachedPolicy.MaxRetries);
        }

        #endregion

        #region WithPolicy(Action<RequestPolicyBuilder>) Tests

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void WithPolicyBuilder_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpRequestMessage request = null!;
            Action<RequestPolicyBuilder> configurePolicy = builder => builder.WithMaxRetries(3);

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.WithPolicy(configurePolicy));
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void WithPolicyBuilder_WhenConfigurePolicyIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            Action<RequestPolicyBuilder> configurePolicy = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.WithPolicy(configurePolicy));
            Assert.Equal("configurePolicy", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void WithPolicyBuilder_WhenValidArguments_ShouldAttachBuiltPolicyToRequest()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            var result = request.WithPolicy(builder => builder
                .WithMaxRetries(7)
                .WithRetryDelay(TimeSpan.FromSeconds(3)));

            // Assert
            Assert.Same(request, result);
            var attachedPolicy = request.GetPolicy();
            Assert.NotNull(attachedPolicy);
            Assert.Equal(7, attachedPolicy.MaxRetries);
            Assert.Equal(TimeSpan.FromSeconds(3), attachedPolicy.RetryDelay);
        }

        #endregion

        #region GetPolicy Tests

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void GetPolicy_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpRequestMessage request = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.GetPolicy());
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void GetPolicy_WhenNoPolicyAttached_ShouldReturnNull()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            var policy = request.GetPolicy();

            // Assert
            Assert.Null(policy);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void GetPolicy_WhenPolicyAttached_ShouldReturnPolicy()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            var expectedPolicy = new RequestPolicy { MaxRetries = 10 };
            request.WithPolicy(expectedPolicy);

            // Act
            var actualPolicy = request.GetPolicy();

            // Assert
            Assert.NotNull(actualPolicy);
            Assert.Same(expectedPolicy, actualPolicy);
        }

        #endregion

        #region HasPolicy Tests

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void HasPolicy_WhenRequestIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            HttpRequestMessage request = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => request.HasPolicy());
            Assert.Equal("request", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void HasPolicy_WhenNoPolicyAttached_ShouldReturnFalse()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");

            // Act
            var hasPolicy = request.HasPolicy();

            // Assert
            Assert.False(hasPolicy);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void HasPolicy_WhenPolicyAttached_ShouldReturnTrue()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            request.WithPolicy(new RequestPolicy());

            // Act
            var hasPolicy = request.HasPolicy();

            // Assert
            Assert.True(hasPolicy);
        }

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void HasPolicy_WhenPolicyAttachedViaBuilder_ShouldReturnTrue()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/test");
            request.WithPolicy(builder => builder.WithMaxRetries(5));

            // Act
            var hasPolicy = request.HasPolicy();

            // Assert
            Assert.True(hasPolicy);
        }

        #endregion

        #region Roundtrip Tests

        [Fact]
        [Trait("Category", "ExtensionMethods")]
        public void PolicyAttachmentRoundtrip_ShouldPreserveAllProperties()
        {
            // Arrange
            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/data");
            var originalPolicy = new RequestPolicy(
                maxRetries: 15,
                retryDelay: TimeSpan.FromMilliseconds(500),
                bypassCircuitBreaker: true,
                disableRetries: false
            );

            // Act
            request.WithPolicy(originalPolicy);
            var retrievedPolicy = request.GetPolicy();

            // Assert
            Assert.NotNull(retrievedPolicy);
            Assert.Equal(15, retrievedPolicy.MaxRetries);
            Assert.Equal(TimeSpan.FromMilliseconds(500), retrievedPolicy.RetryDelay);
            Assert.True(retrievedPolicy.BypassCircuitBreaker);
            Assert.False(retrievedPolicy.DisableRetries);
        }

        #endregion
    }
}
