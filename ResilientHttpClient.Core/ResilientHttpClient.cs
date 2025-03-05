using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading;
using System.Threading.Tasks;

namespace ResilientHttpClient.Core
{
    /// <summary>
    /// A resilient HTTP client that wraps HttpClient and adds common resiliency patterns
    /// such as circuit breaker, retry, and timeout.
    /// </summary>
    public class ResilientHttpClient : IResilientHttpClient
    {
        private readonly HttpClient _httpClient;
        private bool _disposed;
        
        // Circuit breaker state
        private int _failureCount;
        private DateTime _circuitOpenTime = DateTime.MinValue;
        private readonly object _circuitBreakerLock = new object();
        
        // Configuration
        private readonly int _maxFailures;
        private readonly TimeSpan _circuitResetTime;
        private readonly int _maxRetries;
        private readonly TimeSpan _retryDelay;

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientHttpClient"/> class.
        /// </summary>
        /// <param name="httpClient">The HttpClient to wrap.</param>
        public ResilientHttpClient(HttpClient httpClient)
            : this(httpClient, new ResilientHttpClientOptions())
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ResilientHttpClient"/> class with custom options.
        /// </summary>
        /// <param name="httpClient">The HttpClient to wrap.</param>
        /// <param name="options">The options for configuring resiliency patterns.</param>
        public ResilientHttpClient(HttpClient httpClient, ResilientHttpClientOptions options)
        {
            _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
            
            // Set default values if options is null
            options ??= new ResilientHttpClientOptions();
            
            _maxFailures = options.MaxFailures;
            _circuitResetTime = options.CircuitResetTime;
            _maxRetries = options.MaxRetries;
            _retryDelay = options.RetryDelay;
        }

        /// <inheritdoc/>
        public Uri? BaseAddress
        {
            get => _httpClient.BaseAddress;
            set => _httpClient.BaseAddress = value;
        }

        /// <inheritdoc/>
        public HttpRequestHeaders DefaultRequestHeaders => _httpClient.DefaultRequestHeaders;

        /// <inheritdoc/>
        public TimeSpan Timeout
        {
            get => _httpClient.Timeout;
            set => _httpClient.Timeout = value;
        }

        /// <inheritdoc/>
        public long MaxResponseContentBufferSize
        {
            get => _httpClient.MaxResponseContentBufferSize;
            set => _httpClient.MaxResponseContentBufferSize = value;
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request)
        {
            return SendAsync(request, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check if circuit is open
            if (IsCircuitOpen())
            {
                throw new HttpRequestException("Circuit is open due to previous failures");
            }

            // Implement retry logic
            int retryCount = 0;
            while (true)
            {
                try
                {
                    // Clone the request for each retry since it can't be reused
                    var requestClone = retryCount > 0 ? CloneHttpRequestMessage(request) : request;
                    
                    var response = await _httpClient.SendAsync(requestClone, cancellationToken).ConfigureAwait(false);
                    
                    // If successful, reset failure count
                    if (response.IsSuccessStatusCode)
                    {
                        ResetFailureCount();
                        return response;
                    }
                    
                    // Handle transient errors (5xx and certain 4xx)
                    if (IsTransientError(response.StatusCode))
                    {
                        if (retryCount < _maxRetries)
                        {
                            retryCount++;
                            await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                        
                        // If we've exhausted retries, increment failure count
                        IncrementFailureCount();
                    }
                    
                    return response;
                }
                catch (HttpRequestException)
                {
                    // Network errors are considered transient
                    if (retryCount < _maxRetries)
                    {
                        retryCount++;
                        await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    
                    // If we've exhausted retries, increment failure count
                    IncrementFailureCount();
                    throw;
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout, not user cancellation
                    if (retryCount < _maxRetries)
                    {
                        retryCount++;
                        await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    
                    // If we've exhausted retries, increment failure count
                    IncrementFailureCount();
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption)
        {
            return SendAsync(request, completionOption, CancellationToken.None);
        }

        /// <inheritdoc/>
        public async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            // Check if circuit is open
            if (IsCircuitOpen())
            {
                throw new HttpRequestException("Circuit is open due to previous failures");
            }

            // Implement retry logic
            int retryCount = 0;
            while (true)
            {
                try
                {
                    // Clone the request for each retry since it can't be reused
                    var requestClone = retryCount > 0 ? CloneHttpRequestMessage(request) : request;
                    
                    var response = await _httpClient.SendAsync(requestClone, completionOption, cancellationToken).ConfigureAwait(false);
                    
                    // If successful, reset failure count
                    if (response.IsSuccessStatusCode)
                    {
                        ResetFailureCount();
                        return response;
                    }
                    
                    // Handle transient errors (5xx and certain 4xx)
                    if (IsTransientError(response.StatusCode))
                    {
                        if (retryCount < _maxRetries)
                        {
                            retryCount++;
                            await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                            continue;
                        }
                        
                        // If we've exhausted retries, increment failure count
                        IncrementFailureCount();
                    }
                    
                    return response;
                }
                catch (HttpRequestException)
                {
                    // Network errors are considered transient
                    if (retryCount < _maxRetries)
                    {
                        retryCount++;
                        await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    
                    // If we've exhausted retries, increment failure count
                    IncrementFailureCount();
                    throw;
                }
                catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
                {
                    // Timeout, not user cancellation
                    if (retryCount < _maxRetries)
                    {
                        retryCount++;
                        await Task.Delay(_retryDelay, cancellationToken).ConfigureAwait(false);
                        continue;
                    }
                    
                    // If we've exhausted retries, increment failure count
                    IncrementFailureCount();
                    throw;
                }
            }
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(string requestUri)
        {
            return GetAsync(CreateUri(requestUri));
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(Uri requestUri)
        {
            return GetAsync(requestUri, CancellationToken.None);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(string requestUri, CancellationToken cancellationToken)
        {
            return GetAsync(CreateUri(requestUri), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return SendAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption)
        {
            return GetAsync(CreateUri(requestUri), completionOption);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption)
        {
            return GetAsync(requestUri, completionOption, CancellationToken.None);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(string requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            return GetAsync(CreateUri(requestUri), completionOption, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> GetAsync(Uri requestUri, HttpCompletionOption completionOption, CancellationToken cancellationToken)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            using var request = new HttpRequestMessage(HttpMethod.Get, requestUri);
            return SendAsync(request, completionOption, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content)
        {
            return PostAsync(CreateUri(requestUri), content);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content)
        {
            return PostAsync(requestUri, content, CancellationToken.None);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PostAsync(string requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            return PostAsync(CreateUri(requestUri), content, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PostAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using var request = new HttpRequestMessage(HttpMethod.Post, requestUri) { Content = content };
            return SendAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content)
        {
            return PutAsync(CreateUri(requestUri), content);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content)
        {
            return PutAsync(requestUri, content, CancellationToken.None);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PutAsync(string requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            return PutAsync(CreateUri(requestUri), content, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> PutAsync(Uri requestUri, HttpContent content, CancellationToken cancellationToken)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));
            if (content == null)
                throw new ArgumentNullException(nameof(content));

            using var request = new HttpRequestMessage(HttpMethod.Put, requestUri) { Content = content };
            return SendAsync(request, cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> DeleteAsync(string requestUri)
        {
            return DeleteAsync(CreateUri(requestUri));
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> DeleteAsync(Uri requestUri)
        {
            return DeleteAsync(requestUri, CancellationToken.None);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> DeleteAsync(string requestUri, CancellationToken cancellationToken)
        {
            return DeleteAsync(CreateUri(requestUri), cancellationToken);
        }

        /// <inheritdoc/>
        public Task<HttpResponseMessage> DeleteAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            if (requestUri == null)
                throw new ArgumentNullException(nameof(requestUri));

            using var request = new HttpRequestMessage(HttpMethod.Delete, requestUri);
            return SendAsync(request, cancellationToken);
        }

        /// <summary>
        /// Creates a Uri from a string.
        /// </summary>
        /// <param name="uri">The URI string.</param>
        /// <returns>The Uri.</returns>
        private static Uri CreateUri(string uri)
        {
            if (string.IsNullOrEmpty(uri))
                throw new ArgumentNullException(nameof(uri));

            return new Uri(uri, UriKind.RelativeOrAbsolute);
        }

        /// <summary>
        /// Clones an HttpRequestMessage since they can't be reused for multiple requests.
        /// </summary>
        /// <param name="request">The request to clone.</param>
        /// <returns>A new HttpRequestMessage with the same properties.</returns>
        private static HttpRequestMessage CloneHttpRequestMessage(HttpRequestMessage request)
        {
            var clone = new HttpRequestMessage(request.Method, request.RequestUri)
            {
                Content = request.Content,
                Version = request.Version
            };

            foreach (var header in request.Headers)
            {
                clone.Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            foreach (var property in request.Properties)
            {
                clone.Properties.Add(property);
            }

            return clone;
        }

        /// <summary>
        /// Determines if the status code represents a transient error that should be retried.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <returns>True if the status code represents a transient error; otherwise, false.</returns>
        private static bool IsTransientError(HttpStatusCode statusCode)
        {
            // 5xx status codes are server errors and are generally transient
            if ((int)statusCode >= 500 && (int)statusCode < 600)
                return true;

            // Some 4xx status codes can also be transient
            switch (statusCode)
            {
                case HttpStatusCode.RequestTimeout:
                case HttpStatusCode.TooManyRequests: // 429 Too Many Requests
                case HttpStatusCode.GatewayTimeout:
                    return true;
                default:
                    return false;
            }
        }

        /// <summary>
        /// Checks if the circuit breaker is open.
        /// </summary>
        /// <returns>True if the circuit is open; otherwise, false.</returns>
        private bool IsCircuitOpen()
        {
            lock (_circuitBreakerLock)
            {
                // If circuit is open, check if reset time has elapsed
                if (_circuitOpenTime != DateTime.MinValue)
                {
                    if (DateTime.UtcNow - _circuitOpenTime > _circuitResetTime)
                    {
                        // Reset the circuit to half-open state
                        _circuitOpenTime = DateTime.MinValue;
                        return false;
                    }
                    
                    // Circuit is still open
                    return true;
                }
                
                // Circuit is closed
                return false;
            }
        }

        /// <summary>
        /// Increments the failure count and opens the circuit if the threshold is reached.
        /// </summary>
        private void IncrementFailureCount()
        {
            lock (_circuitBreakerLock)
            {
                _failureCount++;
                
                // If we've reached the failure threshold, open the circuit
                if (_failureCount >= _maxFailures)
                {
                    _circuitOpenTime = DateTime.UtcNow;
                }
            }
        }

        /// <summary>
        /// Resets the failure count.
        /// </summary>
        private void ResetFailureCount()
        {
            lock (_circuitBreakerLock)
            {
                _failureCount = 0;
            }
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a string
        /// in an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<string> GetStringAsync(string requestUri)
        {
            return GetStringAsync(CreateUri(requestUri), CancellationToken.None);
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a string
        /// in an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<string> GetStringAsync(Uri requestUri)
        {
            return GetStringAsync(requestUri, CancellationToken.None);
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a string
        /// in an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public Task<string> GetStringAsync(string requestUri, CancellationToken cancellationToken)
        {
            return GetStringAsync(CreateUri(requestUri), cancellationToken);
        }

        /// <summary>
        /// Send a GET request to the specified Uri and return the response body as a string
        /// in an asynchronous operation.
        /// </summary>
        /// <param name="requestUri">The Uri the request is sent to.</param>
        /// <param name="cancellationToken">A cancellation token that can be used by other objects or threads to receive notice of cancellation.</param>
        /// <returns>The task object representing the asynchronous operation.</returns>
        public async Task<string> GetStringAsync(Uri requestUri, CancellationToken cancellationToken)
        {
            using var response = await GetAsync(requestUri, cancellationToken).ConfigureAwait(false);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadAsStringAsync().ConfigureAwait(false);
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases the unmanaged resources used by the <see cref="ResilientHttpClient"/> and optionally releases the managed resources.
        /// </summary>
        /// <param name="disposing">true to release both managed and unmanaged resources; false to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (_disposed)
                return;

            if (disposing)
            {
                _httpClient?.Dispose();
            }

            _disposed = true;
        }
    }
} 