using System;
using System.Net.Http;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// Factory for creating instances of <see cref="ResilientHttpClient"/>.
    /// </summary>
    /// <remarks>
    /// <para>
    /// <strong>IMPORTANT - Instance Reuse:</strong> Each call to CreateClient creates a new HttpClient instance internally.
    /// Creating multiple instances can lead to socket exhaustion and DNS issues.
    /// </para>
    /// <para>
    /// <strong>Recommended Pattern:</strong> Create ONE ResilientHttpClient instance for your application and reuse it for all requests.
    /// ResilientHttpClient is thread-safe and designed to be shared.
    /// </para>
    /// <example>
    /// <code>
    /// // GOOD - Create once, reuse everywhere
    /// private static readonly IResilientHttpClient _client = 
    ///     ResilientHttpClientFactory.CreateClient("https://api.example.com");
    /// 
    /// // BAD - Don't create a new instance for each request
    /// public void MakeRequest() {
    ///     var client = ResilientHttpClientFactory.CreateClient(); // Don't do this!
    /// }
    /// </code>
    /// </example>
    /// </remarks>
    public static class ResilientHttpClientFactory
    {
        /// <summary>
        /// Creates a new instance of <see cref="ResilientHttpClient"/> with default options.
        /// </summary>
        /// <returns>A new instance of <see cref="ResilientHttpClient"/>.</returns>
        /// <remarks>
        /// This method creates a new HttpClient internally. Reuse the returned instance for all requests
        /// to avoid socket exhaustion. See class-level remarks for best practices.
        /// </remarks>
        public static IResilientHttpClient CreateClient()
        {
            return CreateClient(new ResilientHttpClientOptions());
        }

        /// <summary>
        /// Creates a new instance of <see cref="ResilientHttpClient"/> with the specified options.
        /// </summary>
        /// <param name="options">The options for configuring the client.</param>
        /// <returns>A new instance of <see cref="ResilientHttpClient"/>.</returns>
        /// <remarks>
        /// This method creates a new HttpClient internally. Reuse the returned instance for all requests
        /// to avoid socket exhaustion. See class-level remarks for best practices.
        /// </remarks>
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
        /// <remarks>
        /// This method creates a new HttpClient internally with the specified base address.
        /// Reuse the returned instance for all requests to avoid socket exhaustion. See class-level remarks for best practices.
        /// </remarks>
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
        /// <remarks>
        /// This method creates a new HttpClient internally with the specified base address.
        /// Reuse the returned instance for all requests to avoid socket exhaustion. See class-level remarks for best practices.
        /// </remarks>
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