using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ResilientHttpClient.Core;

namespace ResilientHttpClient.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding named ResilientHttpClient instances.
    /// </summary>
    public static class NamedClientExtensions
    {
        /// <summary>
        /// Adds a named ResilientHttpClient to the service collection.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="name">The name of the client.</param>
        /// <param name="baseAddress">The base address for the HTTP client.</param>
        /// <param name="configure">An optional action to configure the client options.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddNamedResilientHttpClient("GitHub", "https://api.github.com", options =>
        /// {
        ///     options.MaxRetries = 5;
        ///     options.RetryDelay = TimeSpan.FromSeconds(2);
        /// });
        /// 
        /// // Inject and use
        /// public class MyService
        /// {
        ///     private readonly IResilientHttpClient _githubClient;
        ///     
        ///     public MyService(IResilientHttpClientFactory factory)
        ///     {
        ///         _githubClient = factory.CreateClient("GitHub");
        ///     }
        /// }
        /// </code>
        /// </example>
        public static IServiceCollection AddNamedResilientHttpClient(
            this IServiceCollection services,
            string name,
            string baseAddress,
            Action<ResilientHttpClientOptions> configure = null)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (string.IsNullOrEmpty(baseAddress))
                throw new ArgumentNullException(nameof(baseAddress));

            // Store configuration for lazy registration
            var options = new ResilientHttpClientOptions();
            configure?.Invoke(options);

            // Ensure factory is registered with all configurations
            services.TryAddSingleton<IResilientHttpClientFactory>(sp =>
            {
                var factory = new ResilientHttpClientFactory();
                
                // Get all configurators and apply them
                var configurators = sp.GetServices<INamedClientConfigurator>();
                foreach (var configurator in configurators)
                {
                    configurator.Configure(factory);
                }
                
                return factory;
            });

            // Add a configurator for this named client
            services.AddSingleton<INamedClientConfigurator>(sp =>
            {
                return new NamedClientConfigurator(name, baseAddress, options);
            });

            return services;
        }
        
        /// <summary>
        /// Configuration for a named client.
        /// </summary>
        private class NamedClientConfiguration
        {
            public string BaseAddress { get; set; }
            public ResilientHttpClientOptions Options { get; set; }
        }

        /// <summary>
        /// Interface for named client configurators.
        /// </summary>
        private interface INamedClientConfigurator
        {
            void Configure(ResilientHttpClientFactory factory);
        }

        /// <summary>
        /// Configurator that registers a named client in the factory.
        /// </summary>
        private class NamedClientConfigurator : INamedClientConfigurator
        {
            private readonly string _name;
            private readonly string _baseAddress;
            private readonly ResilientHttpClientOptions _options;

            public NamedClientConfigurator(string name, string baseAddress, ResilientHttpClientOptions options)
            {
                _name = name;
                _baseAddress = baseAddress;
                _options = options;
            }

            public void Configure(ResilientHttpClientFactory factory)
            {
                factory.RegisterClient(_name, () => Core.ResilientHttpClientFactory.CreateClient(_baseAddress, _options));
            }
        }

        /// <summary>
        /// Adds a named ResilientHttpClient to the service collection with explicit options.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="name">The name of the client.</param>
        /// <param name="options">The client options to use.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// var options = new ResilientHttpClientOptions
        /// {
        ///     MaxRetries = 3,
        ///     CircuitResetTime = TimeSpan.FromSeconds(30)
        /// };
        /// services.AddNamedResilientHttpClient("MyAPI", options);
        /// </code>
        /// </example>
        public static IServiceCollection AddNamedResilientHttpClient(
            this IServiceCollection services,
            string name,
            ResilientHttpClientOptions options)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Ensure factory is registered with all configurations
            services.TryAddSingleton<IResilientHttpClientFactory>(sp =>
            {
                var factory = new ResilientHttpClientFactory();
                
                // Get all configurators and apply them
                var configurators = sp.GetServices<INamedClientConfigurator>();
                foreach (var configurator in configurators)
                {
                    configurator.Configure(factory);
                }
                
                return factory;
            });

            // Add a configurator for this named client
            services.AddSingleton<INamedClientConfigurator>(sp =>
            {
                return new NamedClientConfiguratorWithOptions(name, options);
            });

            return services;
        }
        
        /// <summary>
        /// Configurator for clients with explicit options (no base address).
        /// </summary>
        private class NamedClientConfiguratorWithOptions : INamedClientConfigurator
        {
            private readonly string _name;
            private readonly ResilientHttpClientOptions _options;

            public NamedClientConfiguratorWithOptions(string name, ResilientHttpClientOptions options)
            {
                _name = name;
                _options = options;
            }

            public void Configure(ResilientHttpClientFactory factory)
            {
                factory.RegisterClient(_name, () => Core.ResilientHttpClientFactory.CreateClient(_options));
            }
        }
    }
}
