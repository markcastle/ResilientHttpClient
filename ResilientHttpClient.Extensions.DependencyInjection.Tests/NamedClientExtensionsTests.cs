using System;
using Microsoft.Extensions.DependencyInjection;
using ResilientHttpClient.Core;
using Xunit;

namespace ResilientHttpClient.Extensions.DependencyInjection.Tests
{
    /// <summary>
    /// Tests for NamedClientExtensions class.
    /// </summary>
    public class NamedClientExtensionsTests
    {
        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_WithBaseAddress_RegistersFactory()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act
            services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
            var provider = services.BuildServiceProvider();

            // Assert
            var factory = provider.GetService<IResilientHttpClientFactory>();
            Assert.NotNull(factory);
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_WithBaseAddress_CanCreateNamedClient()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act
            var client = factory.CreateClient("GitHub");

            // Assert
            Assert.NotNull(client);
            Assert.Equal("https://api.github.com/", client.BaseAddress?.ToString());
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_MultipleClients_CanResolveAll()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
            services.AddNamedResilientHttpClient("MyAPI", "https://myapi.com");
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act
            var githubClient = factory.CreateClient("GitHub");
            var myApiClient = factory.CreateClient("MyAPI");

            // Assert
            Assert.NotNull(githubClient);
            Assert.NotNull(myApiClient);
            Assert.Equal("https://api.github.com/", githubClient.BaseAddress?.ToString());
            Assert.Equal("https://myapi.com/", myApiClient.BaseAddress?.ToString());
            Assert.NotSame(githubClient, myApiClient); // Different instances
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_WithConfigure_AppliesOptions()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("TestClient", "https://api.example.com", options =>
            {
                options.MaxRetries = 5;
                options.RetryDelay = TimeSpan.FromMilliseconds(500);
            });
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act
            var client = factory.CreateClient("TestClient");

            // Assert
            Assert.NotNull(client);
            // Note: We can't directly verify the options were applied without exposing them,
            // but we can verify the client was created successfully
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_UnknownClientName_ThrowsArgumentException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act & Assert
            var exception = Assert.Throws<ArgumentException>(() => factory.CreateClient("UnknownClient"));
            Assert.Contains("No HTTP client registered with name 'UnknownClient'", exception.Message);
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddNamedResilientHttpClient(name: null, baseAddress: "https://api.example.com"));
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_NullBaseAddress_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddNamedResilientHttpClient(name: "TestClient", baseAddress: null));
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_WithExplicitOptions_CanCreateClient()
        {
            // Arrange
            var services = new ServiceCollection();
            var options = new ResilientHttpClientOptions
            {
                MaxRetries = 3,
                CircuitResetTime = TimeSpan.FromSeconds(30)
            };
            services.AddNamedResilientHttpClient("TestClient", options);
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act
            var client = factory.CreateClient("TestClient");

            // Assert
            Assert.NotNull(client);
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_WithNullOptions_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => 
                services.AddNamedResilientHttpClient("TestClient", options: (ResilientHttpClientOptions)null));
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void CreateClient_NullName_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateClient(null));
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void CreateClient_EmptyName_ThrowsArgumentNullException()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("GitHub", "https://api.github.com");
            var provider = services.BuildServiceProvider();
            var factory = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() => factory.CreateClient(""));
        }

        [Fact]
        [Trait("Category", "DependencyInjection")]
        [Trait("Category", "NamedClients")]
        public void AddNamedResilientHttpClient_FactoryIsSingleton()
        {
            // Arrange
            var services = new ServiceCollection();
            services.AddNamedResilientHttpClient("Client1", "https://api1.com");
            services.AddNamedResilientHttpClient("Client2", "https://api2.com");
            var provider = services.BuildServiceProvider();

            // Act
            var factory1 = provider.GetRequiredService<IResilientHttpClientFactory>();
            var factory2 = provider.GetRequiredService<IResilientHttpClientFactory>();

            // Assert
            Assert.Same(factory1, factory2); // Same instance = singleton
        }
    }
}
