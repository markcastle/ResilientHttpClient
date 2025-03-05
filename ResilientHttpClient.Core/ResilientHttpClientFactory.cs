using System;
using System.Net.Http;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// Factory for creating instances of <see cref="ResilientHttpClient"/>.
    /// </summary>
    public static class ResilientHttpClientFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ResilientHttpClient"/> with default options.
        /// </summary>
        /// <returns>A new instance of <see cref="ResilientHttpClient"/>.</returns>
        public static IResilientHttpClient CreateClient()
        {
            return CreateClient(new ResilientHttpClientOptions());
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResilientHttpClient"/> with the specified options.
        /// </summary>
        /// <param name="options">The options for configuring the client.</param>
        /// <returns>A new instance of <see cref="ResilientHttpClient"/>.</returns>
        public static IResilientHttpClient CreateClient(ResilientHttpClientOptions options)
        {
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var httpClient = new HttpClient();
            return new ResilientHttpClient(httpClient, options);
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResilientHttpClient"/> with the specified base address and default options.
        /// </summary>
        /// <param name="baseAddress">The base address for the client.</param>
        /// <returns>A new instance of <see cref="ResilientHttpClient"/>.</returns>
        public static IResilientHttpClient CreateClient(string baseAddress)
        {
            return CreateClient(baseAddress, new ResilientHttpClientOptions());
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResilientHttpClient"/> with the specified base address and options.
        /// </summary>
        /// <param name="baseAddress">The base address for the client.</param>
        /// <param name="options">The options for configuring the client.</param>
        /// <returns>A new instance of <see cref="ResilientHttpClient"/>.</returns>
        public static IResilientHttpClient CreateClient(string baseAddress, ResilientHttpClientOptions options)
        {
            if (string.IsNullOrEmpty(baseAddress))
                throw new ArgumentNullException(nameof(baseAddress));
            if (options == null)
                throw new ArgumentNullException(nameof(options));

            var httpClient = new HttpClient
            {
                BaseAddress = new Uri(baseAddress)
            };

            return new ResilientHttpClient(httpClient, options);
        }
    }
} 