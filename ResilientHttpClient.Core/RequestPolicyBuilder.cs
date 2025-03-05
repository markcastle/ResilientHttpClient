using System;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// A fluent builder for creating request-specific resilience policies.
    /// </summary>
    public class RequestPolicyBuilder
    {
        private readonly RequestPolicy _policy = new RequestPolicy();

        /// <summary>
        /// Creates a new instance of the <see cref="RequestPolicyBuilder"/> class.
        /// </summary>
        public RequestPolicyBuilder()
        {
        }

        /// <summary>
        /// Sets the maximum number of retries for transient failures.
        /// </summary>
        /// <param name="maxRetries">The maximum number of retries.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public RequestPolicyBuilder WithMaxRetries(int maxRetries)
        {
            _policy.MaxRetries = maxRetries;
            return this;
        }

        /// <summary>
        /// Sets the delay between retry attempts.
        /// </summary>
        /// <param name="retryDelay">The delay between retry attempts.</param>
        /// <returns>The builder instance for method chaining.</returns>
        public RequestPolicyBuilder WithRetryDelay(TimeSpan retryDelay)
        {
            _policy.RetryDelay = retryDelay;
            return this;
        }

        /// <summary>
        /// Configures the request to bypass the circuit breaker.
        /// </summary>
        /// <returns>The builder instance for method chaining.</returns>
        public RequestPolicyBuilder BypassCircuitBreaker()
        {
            _policy.BypassCircuitBreaker = true;
            return this;
        }

        /// <summary>
        /// Disables retries for this request.
        /// </summary>
        /// <returns>The builder instance for method chaining.</returns>
        public RequestPolicyBuilder DisableRetries()
        {
            _policy.DisableRetries = true;
            return this;
        }

        /// <summary>
        /// Builds the request policy.
        /// </summary>
        /// <returns>The configured request policy.</returns>
        public RequestPolicy Build()
        {
            return _policy;
        }
    }
} 