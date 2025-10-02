using System;
using System.Collections.Concurrent;
using ResilientHttpClient.Core;

namespace ResilientHttpClient.Extensions.DependencyInjection
{
    /// <summary>
    /// Default implementation of IResilientHttpClientFactory that creates named clients.
    /// </summary>
    internal class ResilientHttpClientFactory : IResilientHttpClientFactory
    {
        private readonly ConcurrentDictionary<string, Func<IResilientHttpClient>> _clientFactories;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientHttpClientFactory"/> class.
        /// </summary>
        public ResilientHttpClientFactory()
        {
            _clientFactories = new ConcurrentDictionary<string, Func<IResilientHttpClient>>(StringComparer.OrdinalIgnoreCase);
        }

        /// <summary>
        /// Registers a factory function for creating a named client.
        /// </summary>
        /// <param name="name">The name of the client.</param>
        /// <param name="factory">A function that creates the client instance.</param>
        internal void RegisterClient(string name, Func<IResilientHttpClient> factory)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));
            if (factory == null)
                throw new ArgumentNullException(nameof(factory));

            _clientFactories.TryAdd(name, factory);
        }

        /// <inheritdoc/>
        public IResilientHttpClient CreateClient(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException(nameof(name));

            if (!_clientFactories.TryGetValue(name, out var factory))
            {
                throw new ArgumentException($"No HTTP client registered with name '{name}'. " +
                    $"Available clients: {string.Join(", ", _clientFactories.Keys)}");
            }

            return factory();
        }
    }
}
