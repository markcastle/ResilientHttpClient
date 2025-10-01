using System;
using ResilientHttpClient.Core;
using Xunit;

namespace ResilientHttpClient.Tests
{
    /// <summary>
    /// Tests for ResilientHttpClientFactory and ResilientHttpClientOptions
    /// to ensure proper initialization and validation.
    /// </summary>
    public class FactoryAndOptionsTests
    {
        #region Factory Tests

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithoutArguments_ShouldCreateClientWithDefaultOptions()
        {
            // Act
            var client = ResilientHttpClientFactory.CreateClient();

            // Assert
            Assert.NotNull(client);
            Assert.Null(client.BaseAddress); // No base address specified
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithOptions_WhenOptionsIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            ResilientHttpClientOptions options = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                ResilientHttpClientFactory.CreateClient(options));
            Assert.Equal("options", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithValidOptions_ShouldCreateClient()
        {
            // Arrange
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 5,
                RetryDelay = TimeSpan.FromSeconds(2),
                MaxFailures = 10,
                CircuitResetTime = TimeSpan.FromMinutes(1)
            };

            // Act
            var client = ResilientHttpClientFactory.CreateClient(options);

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithBaseAddress_WhenBaseAddressIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            string baseAddress = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                ResilientHttpClientFactory.CreateClient(baseAddress));
            Assert.Equal("baseAddress", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithBaseAddress_WhenBaseAddressIsEmpty_ShouldThrowArgumentNullException()
        {
            // Arrange
            string baseAddress = string.Empty;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                ResilientHttpClientFactory.CreateClient(baseAddress));
            Assert.Equal("baseAddress", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithValidBaseAddress_ShouldSetBaseAddressCorrectly()
        {
            // Arrange
            string baseAddress = "https://api.example.com/";

            // Act
            var client = ResilientHttpClientFactory.CreateClient(baseAddress);

            // Assert
            Assert.NotNull(client);
            Assert.Equal(new Uri(baseAddress), client.BaseAddress);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithBaseAddressAndOptions_WhenBaseAddressIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            string baseAddress = null!;
            var options = new ResilientHttpClientOptions();

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                ResilientHttpClientFactory.CreateClient(baseAddress, options));
            Assert.Equal("baseAddress", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithBaseAddressAndOptions_WhenOptionsIsNull_ShouldThrowArgumentNullException()
        {
            // Arrange
            string baseAddress = "https://api.example.com/";
            ResilientHttpClientOptions options = null!;

            // Act & Assert
            var exception = Assert.Throws<ArgumentNullException>(() => 
                ResilientHttpClientFactory.CreateClient(baseAddress, options));
            Assert.Equal("options", exception.ParamName);
        }

        [Fact]
        [Trait("Category", "Factory")]
        public void CreateClient_WithBaseAddressAndOptions_WhenBothValid_ShouldCreateClient()
        {
            // Arrange
            string baseAddress = "https://api.example.com/";
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                RetryDelay = TimeSpan.FromSeconds(1)
            };

            // Act
            var client = ResilientHttpClientFactory.CreateClient(baseAddress, options);

            // Assert
            Assert.NotNull(client);
            Assert.Equal(new Uri(baseAddress), client.BaseAddress);
        }

        #endregion

        #region Options Tests

        [Fact]
        [Trait("Category", "Options")]
        public void Options_DefaultValues_ShouldBeSetCorrectly()
        {
            // Act
            var options = new ResilientHttpClientOptions();

            // Assert
            Assert.Equal(5, options.MaxFailures);
            Assert.Equal(TimeSpan.FromSeconds(30), options.CircuitResetTime);
            Assert.Equal(3, options.MaxRetries);
            Assert.Equal(TimeSpan.FromSeconds(1), options.RetryDelay);
        }

        [Fact]
        [Trait("Category", "Options")]
        public void Options_SetMaxFailures_ShouldUpdateValue()
        {
            // Arrange
            var options = new ResilientHttpClientOptions();

            // Act
            options.MaxFailures = 10;

            // Assert
            Assert.Equal(10, options.MaxFailures);
        }

        [Fact]
        [Trait("Category", "Options")]
        public void Options_SetCircuitResetTime_ShouldUpdateValue()
        {
            // Arrange
            var options = new ResilientHttpClientOptions();

            // Act
            options.CircuitResetTime = TimeSpan.FromMinutes(5);

            // Assert
            Assert.Equal(TimeSpan.FromMinutes(5), options.CircuitResetTime);
        }

        [Fact]
        [Trait("Category", "Options")]
        public void Options_SetMaxRetries_ShouldUpdateValue()
        {
            // Arrange
            var options = new ResilientHttpClientOptions();

            // Act
            options.MaxRetries = 7;

            // Assert
            Assert.Equal(7, options.MaxRetries);
        }

        [Fact]
        [Trait("Category", "Options")]
        public void Options_SetRetryDelay_ShouldUpdateValue()
        {
            // Arrange
            var options = new ResilientHttpClientOptions();

            // Act
            options.RetryDelay = TimeSpan.FromMilliseconds(500);

            // Assert
            Assert.Equal(TimeSpan.FromMilliseconds(500), options.RetryDelay);
        }

        [Fact]
        [Trait("Category", "Options")]
        public void Options_InitializerSyntax_ShouldSetAllProperties()
        {
            // Act
            var options = new ResilientHttpClientOptions
            {
                MaxFailures = 15,
                CircuitResetTime = TimeSpan.FromMinutes(2),
                MaxRetries = 8,
                RetryDelay = TimeSpan.FromMilliseconds(750)
            };

            // Assert
            Assert.Equal(15, options.MaxFailures);
            Assert.Equal(TimeSpan.FromMinutes(2), options.CircuitResetTime);
            Assert.Equal(8, options.MaxRetries);
            Assert.Equal(TimeSpan.FromMilliseconds(750), options.RetryDelay);
        }

        #endregion
    }
}
