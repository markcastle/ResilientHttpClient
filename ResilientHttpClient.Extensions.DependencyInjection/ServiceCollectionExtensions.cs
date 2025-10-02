using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;
using ResilientHttpClient.Core;

namespace ResilientHttpClient.Extensions.DependencyInjection
{
    /// <summary>
    /// Extension methods for adding ResilientHttpClient to IServiceCollection.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds a singleton ResilientHttpClient to the service collection with default options.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddResilientHttpClient();
        /// </code>
        /// </example>
        public static IServiceCollection AddResilientHttpClient(this IServiceCollection services)
        {
            return AddResilientHttpClient(services, configure: null, lifetime: ServiceLifetime.Singleton);
        }

        /// <summary>
        /// Adds a ResilientHttpClient to the service collection with custom options.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="configure">An action to configure the client options.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddResilientHttpClient(options =>
        /// {
        ///     options.MaxRetries = 5;
        ///     options.CircuitResetTime = TimeSpan.FromSeconds(30);
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddResilientHttpClient(
            this IServiceCollection services,
            Action<ResilientHttpClientOptions> configure,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));

            // Configure options if provided
            if (configure != null)
            {
                services.Configure(configure);
            }

            // Register the client with the specified lifetime
            var descriptor = new ServiceDescriptor(
                typeof(IResilientHttpClient),
                provider =>
                {
                    var options = provider.GetService<IOptions<ResilientHttpClientOptions>>()?.Value 
                                  ?? new ResilientHttpClientOptions();
                    return Core.ResilientHttpClientFactory.CreateClient(options);
                },
                lifetime);

            services.TryAdd(descriptor);

            return services;
        }

        /// <summary>
        /// Adds a ResilientHttpClient to the service collection with a base address and custom options.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="baseAddress">The base address for the HTTP client.</param>
        /// <param name="configure">An optional action to configure the client options.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// services.AddResilientHttpClient("https://api.example.com", options =>
        /// {
        ///     options.MaxRetries = 3;
        ///     options.RetryDelay = TimeSpan.FromMilliseconds(500);
        /// });
        /// </code>
        /// </example>
        public static IServiceCollection AddResilientHttpClient(
            this IServiceCollection services,
            string baseAddress,
            Action<ResilientHttpClientOptions> configure = null,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrEmpty(baseAddress))
                throw new ArgumentNullException(nameof(baseAddress));

            // Configure options
            services.Configure<ResilientHttpClientOptions>(options =>
            {
                configure?.Invoke(options);
            });

            // Register the client with the specified lifetime
            var descriptor = new ServiceDescriptor(
                typeof(IResilientHttpClient),
                provider =>
                {
                    var options = provider.GetService<IOptions<ResilientHttpClientOptions>>()?.Value 
                                  ?? new ResilientHttpClientOptions();
                    return Core.ResilientHttpClientFactory.CreateClient(baseAddress, options);
                },
                lifetime);

            services.TryAdd(descriptor);

            return services;
        }

        /// <summary>
        /// Adds a ResilientHttpClient to the service collection with explicit options instance.
        /// </summary>
        /// <param name="services">The service collection to add services to.</param>
        /// <param name="options">The client options to use.</param>
        /// <param name="lifetime">The service lifetime (default: Singleton).</param>
        /// <returns>The service collection for chaining.</returns>
        /// <example>
        /// <code>
        /// var options = new ResilientHttpClientOptions
        /// {
        ///     MaxRetries = 3,
        ///     CircuitResetTime = TimeSpan.FromSeconds(30)
        /// };
        /// services.AddResilientHttpClient(options);
        /// </code>
        /// </example>
        public static IServiceCollection AddResilientHttpClient(
            this IServiceCollection services,
            ResilientHttpClientOptions options,
            ServiceLifetime lifetime = ServiceLifetime.Singleton)
        {
            if (services == null)
                throw new ArgumentNullException(nameof(services));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            // Register the options
            services.TryAddSingleton(Options.Create(options));

            // Register the client with the specified lifetime
            var descriptor = new ServiceDescriptor(
                typeof(IResilientHttpClient),
                provider => Core.ResilientHttpClientFactory.CreateClient(options),
                lifetime);

            services.TryAdd(descriptor);

            return services;
        }
    }
}
