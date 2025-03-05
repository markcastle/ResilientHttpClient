using System;
using System.Net.Http;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// Extension methods for <see cref="HttpRequestMessage"/> to work with resilience policies.
    /// </summary>
    public static class HttpRequestMessageExtensions
    {
        private const string PolicyPropertyKey = "ResilientHttpClient_RequestPolicy";

        /// <summary>
        /// Attaches a resilience policy to the request.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="policy">The resilience policy to attach.</param>
        /// <returns>The HTTP request message with the attached policy.</returns>
        public static HttpRequestMessage WithPolicy(this HttpRequestMessage request, RequestPolicy policy)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (policy == null)
                throw new ArgumentNullException(nameof(policy));

            request.Properties[PolicyPropertyKey] = policy;
            return request;
        }

        /// <summary>
        /// Configures a resilience policy for the request using a fluent builder.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <param name="configurePolicy">An action to configure the policy using the builder.</param>
        /// <returns>The HTTP request message with the attached policy.</returns>
        public static HttpRequestMessage WithPolicy(this HttpRequestMessage request, Action<RequestPolicyBuilder> configurePolicy)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));
            if (configurePolicy == null)
                throw new ArgumentNullException(nameof(configurePolicy));

            var builder = new RequestPolicyBuilder();
            configurePolicy(builder);
            var policy = builder.Build();

            return request.WithPolicy(policy);
        }

        /// <summary>
        /// Gets the resilience policy attached to the request, if any.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>The attached policy, or null if no policy is attached.</returns>
        public static RequestPolicy? GetPolicy(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (request.Properties.TryGetValue(PolicyPropertyKey, out var policyObj) && policyObj is RequestPolicy policy)
            {
                return policy;
            }

            return null;
        }

        /// <summary>
        /// Checks if the request has a resilience policy attached.
        /// </summary>
        /// <param name="request">The HTTP request message.</param>
        /// <returns>True if a policy is attached, false otherwise.</returns>
        public static bool HasPolicy(this HttpRequestMessage request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            return request.Properties.ContainsKey(PolicyPropertyKey);
        }
    }
} 