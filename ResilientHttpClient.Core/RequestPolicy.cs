using System;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// Represents resilience policies that can be applied to individual HTTP requests.
    /// </summary>
    public class RequestPolicy
    {
        /// <summary>
        /// Gets or sets the maximum number of retries for transient failures.
        /// If null, the client's default setting will be used.
        /// </summary>
        public int? MaxRetries { get; set; }

        /// <summary>
        /// Gets or sets the delay between retry attempts.
        /// If null, the client's default setting will be used.
        /// </summary>
        public TimeSpan? RetryDelay { get; set; }

        /// <summary>
        /// Gets or sets whether to bypass the circuit breaker for this request.
        /// </summary>
        public bool BypassCircuitBreaker { get; set; }

        /// <summary>
        /// Gets or sets whether to disable retries for this request.
        /// </summary>
        public bool DisableRetries { get; set; }

        /// <summary>
        /// Creates a new instance of the <see cref="RequestPolicy"/> class.
        /// </summary>
        public RequestPolicy()
        {
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RequestPolicy"/> class with the specified settings.
        /// </summary>
        /// <param name="maxRetries">The maximum number of retries for transient failures.</param>
        /// <param name="retryDelay">The delay between retry attempts.</param>
        /// <param name="bypassCircuitBreaker">Whether to bypass the circuit breaker for this request.</param>
        /// <param name="disableRetries">Whether to disable retries for this request.</param>
        public RequestPolicy(int? maxRetries, TimeSpan? retryDelay, bool bypassCircuitBreaker = false, bool disableRetries = false)
        {
            MaxRetries = maxRetries;
            RetryDelay = retryDelay;
            BypassCircuitBreaker = bypassCircuitBreaker;
            DisableRetries = disableRetries;
        }
    }
} 