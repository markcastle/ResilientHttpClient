using System;
using Microsoft.Extensions.DependencyInjection;
using ResilientHttpClient.Core;
using Xunit;

namespace ResilientHttpClient.Extensions.DependencyInjection.Tests
{
    /// <summary>
    /// Tests for ServiceCollectionExtensions class.
    /// </summary>
    public class ServiceCollectionExtensionsTests
    {
        #region AddResilientHttpClient() - Default

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_Default_RegistersClient()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResilientHttpClient();
            var provider = services.BuildServiceProvider();

            // Assert
            var client = provider.GetService<IResilientHttpClient>();
            Assert.NotNull(client);
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_Default_RegistersAsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddResilientHttpClient();
            var provider = services.BuildServiceProvider();

            // Act
            var client1 = provider.GetService<IResilientHttpClient>();
            var client2 = provider.GetService<IResilientHttpClient>();

            // Assert
            Assert.Same(client1, client2); // Same instance = singleton
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_NullServices_ThrowsArgumentNullException()
        {
            // Arrange
            IServiceCollection services = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => services.AddResilientHttpClient());
        }

        #endregion

        #region AddResilientHttpClient(configure) - With Options

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithConfigure_RegistersClient()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddResilientHttpClient(options =>
            {
                options.MaxRetries = 5;
                options.CircuitResetTime = TimeSpan.FromSeconds(60);
            });
            var provider = services.BuildServiceProvider();

            // Assert
            var client = provider.GetService<IResilientHttpClient>();
            Assert.NotNull(client);
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithScoped_RegistersAsScoped()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddResilientHttpClient(
                configure: options => { },
                lifetime: ServiceLifetime.Scoped);
            var provider = services.BuildServiceProvider();

            // Act
            IResilientHttpClient client1, client2;
            using (var scope1 = provider.CreateScope())
            {
                client1 = scope1.ServiceProvider.GetRequiredService<IResilientHttpClient>();
            }
            using (var scope2 = provider.CreateScope())
            {
                client2 = scope2.ServiceProvider.GetRequiredService<IResilientHttpClient>();
            }

            // Assert
            Assert.NotSame(client1, client2); // Different instances across scopes
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithTransient_RegistersAsTransient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddResilientHttpClient(
                configure: options => { },
                lifetime: ServiceLifetime.Transient);
            var provider = services.BuildServiceProvider();

            // Act
            var client1 = provider.GetRequiredService<IResilientHttpClient>();
            var client2 = provider.GetRequiredService<IResilientHttpClient>();

            // Assert
            Assert.NotSame(client1, client2); // Different instances = transient
        }

        #endregion

        #region AddResilientHttpClient(baseAddress, configure) - With Base Address

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithBaseAddress_RegistersClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var baseAddress = "https://api.example.com";

            // Act
            services.AddResilientHttpClient(baseAddress);
            var provider = services.BuildServiceProvider();

            // Assert
            var client = provider.GetService<IResilientHttpClient>();
            Assert.NotNull(client);
            Assert.Equal(baseAddress + "/", client.BaseAddress?.ToString());
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithBaseAddressAndConfigure_RegistersClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var baseAddress = "https://api.example.com";

            // Act
            services.AddResilientHttpClient(baseAddress, options =>
            {
                options.MaxRetries = 5;
                options.RetryDelay = TimeSpan.FromMilliseconds(500);
            });
            var provider = services.BuildServiceProvider();

            // Assert
            var client = provider.GetService<IResilientHttpClient>();
            Assert.NotNull(client);
            Assert.Equal(baseAddress + "/", client.BaseAddress?.ToString());
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithNullBaseAddress_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddResilientHttpClient(baseAddress: null));
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithEmptyBaseAddress_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddResilientHttpClient(baseAddress: ""));
        }

        #endregion

        #region AddResilientHttpClient(options) - With Explicit Options

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithExplicitOptions_RegistersClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                CircuitResetTime = TimeSpan.FromSeconds(30)
            };

            // Act
            services.AddResilientHttpClient(options);
            var provider = services.BuildServiceProvider();

            // Assert
            var client = provider.GetService<IResilientHttpClient>();
            Assert.NotNull(client);
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddResilientHttpClient(options: (ResilientHttpClientOptions)null));
        }

        #endregion

        #region Multiple Registrations

        [Fact]
        [Trait("Category", "DependencyInjection")]
        public void AddResilientHttpClient_MultipleCalls_UsesFirstRegistration()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddResilientHttpClient("https://first.com");
            services.AddResilientHttpClient("https://second.com"); // Should not override
            var provider = services.BuildServiceProvider();

            // Act
            var client = provider.GetRequiredService<IResilientHttpClient>();

            // Assert
            // TryAdd behavior means first registration wins
            Assert.Equal("https://first.com/", client.BaseAddress?.ToString());
        }

        #endregion
    }
}
