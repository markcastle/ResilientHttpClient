# TASK.md – Code Coverage Improvement Roadmap

## Epic 1: Achieve 100% Line and Branch Code Coverage

### Why?
Improving code coverage ensures reliability, maintainability, and confidence in the ResilientHttpClient library. This epic will guide us step-by-step toward 100% test coverage.

---

## Epic 1.1: Audit Current Coverage
- [ ] Review the latest coverage report and list all uncovered files, classes, and methods.
- [ ] Identify critical logic and branches that are not tested.

## Epic 1.1: Audit Current Coverage (Detailed – ResilientHttpClient)

### Constructor Logic
- [x] Test default constructor with valid HttpClient
- [x] Test constructor with null HttpClient (should throw)
- [x] Test constructor with custom ResilientHttpClientOptions
- [x] Test constructor with null options (should use defaults)

### SendAsync / Retry / Circuit Breaker
- [x] Test normal successful request
- [x] Test transient failure with retry logic (simulate 5xx, 408, etc.)
- [ ] Test retry exhaustion (should increment failure count and eventually open circuit)
- [x] Test circuit breaker opens after max failures
- [x] Test request when circuit is open (should throw immediately)
- [x] Test bypassing circuit breaker with request policy
- [x] Test timeout handling (simulate TaskCanceledException when not user-cancelled)
- [x] Test cancellation token cancels request (user-initiated cancellation)
- [x] Test error handling for null request
- [x] Test circuit reset after timeout period (half-open state)
- [x] Test SendAsync with HttpCompletionOption overloads

### GetAsync / PostAsync / PutAsync / DeleteAsync
- [x] Test GetAsync overloads for null URI arguments
- [x] Test PostAsync overloads for null URI/content arguments
- [x] Test PutAsync overloads for null URI/content arguments
- [x] Test DeleteAsync overloads for null URI arguments
- [x] Test correct HTTP method is used for each verb
- [x] Test all overloads with cancellation tokens
- [x] Test all overloads with HttpCompletionOption

### GetStringAsync
- [x] Test with valid URIs (string and Uri overloads)
- [x] Test with null URI arguments (should throw)
- [x] Test that response body is returned as string
- [x] Test error/exception cases (non-success status calls EnsureSuccessStatusCode)
- [x] Test with cancellation token

### Dispose Pattern
- [x] Test Dispose releases resources
- [x] Test double-dispose does not throw
- [x] Test using pattern (using var client = ...)

### Private Helpers (via public API)
- [x] Test IncrementFailureCount and ResetFailureCount via public API (circuit breaker, retry)
- [x] Test CreateUri with valid/invalid strings (via GetAsync, PostAsync, etc.)
- [x] Test CloneHttpRequestMessage preserves headers, properties, content (via retry scenarios)

## Epic 1.2: Cover Core Client Logic
- [x] Test all public properties (BaseAddress, DefaultRequestHeaders, Timeout, MaxResponseContentBufferSize)
- [x] Test IsTransientError logic for all relevant status codes
- [x] Test circuit breaker state transitions (closed -> open -> half-open -> closed)
- [x] Test all retry scenarios with different MaxRetries values
- [x] Test all delay scenarios with different RetryDelay values

## Epic 1.3: Cover Extension Methods
- [x] Test WithPolicy(RequestPolicy) with null request/policy arguments
- [x] Test WithPolicy(Action<RequestPolicyBuilder>) with null request/configurePolicy arguments
- [x] Test GetPolicy() with null request and with/without attached policy
- [x] Test HasPolicy() with null request and with/without attached policy
- [x] Test policy attachment and retrieval roundtrip

## Epic 1.4: Cover Policy and Builder Logic
- [x] Test RequestPolicy constructors (default and parameterized)
- [x] Test RequestPolicyBuilder fluent methods (WithMaxRetries, WithRetryDelay, BypassCircuitBreaker, DisableRetries)
- [x] Test RequestPolicyBuilder.Build() returns correct policy
- [x] Test method chaining in RequestPolicyBuilder

## Epic 1.5: Cover Factory and Options Classes
- [x] Test ResilientHttpClientFactory.CreateClient() (default options)
- [x] Test ResilientHttpClientFactory.CreateClient(options) with null options
- [x] Test ResilientHttpClientFactory.CreateClient(baseAddress) with null/empty baseAddress
- [x] Test ResilientHttpClientFactory.CreateClient(baseAddress, options) with null arguments
- [x] Test ResilientHttpClientOptions default values
- [x] Test ResilientHttpClientOptions property setters

## Epic 1.6: Increase Branch Coverage
- [ ] Review all `if`, `else`, and `switch` statements across the codebase
- [ ] Ensure both true/false and all case branches are tested
- [ ] Add tests to cover exception paths and rare error conditions

## Epic 1.7: Continuous Validation
- [x] Rerun coverage after each round of new tests
- [x] Update this checklist as coverage improves
- [x] Mark each subtask as complete when coverage for that area reaches 80%+

---

## Discovered During Work
- [ ] (Add any new tasks or edge cases found during test writing here)

---

## Progress Tracking
- [x] 82.6% line coverage and 79.4% branch coverage achieved
- [x] 80 tests passing (up from 19)
- [x] CI/CD pipeline configured
- [x] NuGet package metadata complete

---

**Instructions:**
- Use checkboxes to track progress.
- Update this file as you add tests and improve coverage.
- Add new subtasks if new uncovered areas are discovered.
