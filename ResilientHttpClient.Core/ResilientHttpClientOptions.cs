using System;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// Options for configuring the <see cref="ResilientHttpClient"/>.
    /// </summary>
    public class ResilientHttpClientOptions
    {
        /// <summary>
        /// Gets or sets the maximum number of failures before the circuit breaker opens.
        /// Default is 5.
        /// </summary>
        public int MaxFailures { get; set; } = 5;

        /// <summary>
        /// Gets or sets the time to keep the circuit breaker open before allowing a trial request.
        /// Default is 30 seconds.
        /// </summary>
        public TimeSpan CircuitResetTime { get; set; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets or sets the maximum number of retries for transient failures.
        /// Default is 3.
        /// </summary>
        public int MaxRetries { get; set; } = 3;

        /// <summary>
        /// Gets or sets the delay between retry attempts.
        /// Default is 1 second.
        /// </summary>
        public TimeSpan RetryDelay { get; set; } = TimeSpan.FromSeconds(1);
    }
} 