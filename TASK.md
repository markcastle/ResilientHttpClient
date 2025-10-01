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

## Epic 1.8: Push to 90%+ Coverage (Target: 90-95%)

### ✅ ACHIEVED: 91.7% line, 86.0% branch
### 🎯 Original Target: 90%+ line, 85%+ branch - **EXCEEDED!**

### Identified Gaps from Coverage Report

#### HTTP Verb Methods - Missing HttpCompletionOption Overloads (0% coverage)
**File**: ResilientHttpClient.cs

- [x] **GetAsync(string, HttpCompletionOption)** - Lines 309-311 (0 hits)
  - Test: Call with valid URI and HttpCompletionOption.ResponseHeadersRead ✅
  - Test: Call with valid URI and HttpCompletionOption.ResponseContentRead ✅
  
- [x] **GetAsync(Uri, HttpCompletionOption)** - Lines 315-317 (0 hits)
  - Test: Call with Uri object and HttpCompletionOption ✅
  
- [x] **GetAsync(string, HttpCompletionOption, CancellationToken)** - Lines 321-323 (0 hits)
  - Test: Call with all parameters including cancellation token ✅
  
- [x] **GetAsync(Uri, HttpCompletionOption, CancellationToken)** - Lines 327-333 (0 hits, 0% branch)
  - Test: Call with null URI (should throw) ✅
  - Test: Call with valid parameters ✅
  - **Priority**: This has untested branches! ✅

- [x] **PostAsync(string, HttpContent, CancellationToken)** - Lines 349-351 (0 hits)
  - Test: Call with cancellation token parameter ✅
  
- [x] **PutAsync(string, HttpContent, CancellationToken)** - Lines 379-381 (0 hits)
  - Test: Call with cancellation token parameter ✅
  
- [x] **PutAsync(Uri, HttpContent, CancellationToken)** - Line 387 (0 hits, partial branch)
  - Test: Call with null URI to hit missing branch ✅
  
- [x] **DeleteAsync(string, CancellationToken)** - Lines 409-411 (0 hits)
  - Test: Call with cancellation token parameter ✅
  
- [x] **DeleteAsync(Uri, CancellationToken)** - Line 417 (0 hits, partial branch)
  - Test: Call with null URI to hit missing branch ✅

#### Request Cloning - Missing Header Branch (81.25% coverage)
**File**: ResilientHttpClient.cs, CloneHttpRequestMessage method

- [x] **Lines 449-452** - Header cloning loop (0 hits, 50% branch)
  - Test: Create retry scenario with request that has custom headers ✅
  - Test: Verify headers are cloned correctly during retry ✅
  - **Reason**: Current tests don't use requests with headers during retries - **FIXED**

#### SendAsync with HttpCompletionOption - Partial Coverage
**File**: ResilientHttpClient.cs

- [x] **SendAsync retry with HttpCompletionOption** - Test all retry paths with HttpCompletionOption parameter
  - Test: Transient failure with HttpCompletionOption.ResponseHeadersRead ✅
  - Test: Circuit breaker behavior with HttpCompletionOption ✅

### Actual Impact Achieved ✅
- **HttpCompletionOption overloads**: +6.2% line coverage
- **Request cloning with headers**: +1.8% line coverage, +3.6% branch coverage
- **Additional edge cases**: +1.1% line coverage

**Total Achieved: 91.7% line coverage (+9.1%), 86.0% branch coverage (+6.6%)**

### New Test Files Created
- **HttpCompletionOptionTests.cs** (14 tests) - All HTTP verb HttpCompletionOption overloads
- **RequestCloningTests.cs** (6 tests) - Request header/property cloning during retries

---

## Discovered During Work
- [ ] Consider testing GetStringAsync with very large responses
- [ ] Consider testing concurrent requests to circuit breaker

---

## Epic 1.9: Maximum Coverage Achievement - 95%+ 🏆

### ✅ ACHIEVED: 95.2% line, 88.97% branch
### 🎯 Target: 95%+ line coverage - **ACHIEVED!**

### Final Gaps Eliminated

- [x] **GetStringAsync(Uri)** - Lines 558-560 (was 0% coverage)
  - Test: Basic call with Uri object ✅
  - Test: Retry scenario with Uri object ✅  
  - Test: Null Uri validation ✅
  - Test: Error handling (EnsureSuccessStatusCode) ✅
  - Test: Cancellation support ✅

- [x] **Dispose edge cases** - Line 606 branch (was 50% coverage)
  - Test: Dispose with disposed HttpClient ✅
  - Test: Protected Dispose(false) from finalizer path ✅

### New Test File Created
- **FinalCoverageTests.cs** (8 tests) - GetStringAsync(Uri) overload + Dispose edge cases

### Actual Impact Achieved ✅
- **GetStringAsync(Uri) overload**: +2.8% line coverage
- **Dispose edge cases**: +0.7% line coverage, +2.97% branch coverage

**Total Achieved: 95.2% line coverage (+3.5% from Session 2), 88.97% branch coverage (+2.97%)**

---

## Progress Tracking
- [x] 82.6% line coverage and 79.4% branch coverage achieved (Session 1)
- [x] 91.7% line coverage and 86.0% branch coverage achieved (Session 2)
- [x] **95.2% line coverage and 88.97% branch coverage achieved (Session 3)** 🏆
- [x] 104 tests passing (up from 19 original, +85 new tests)
- [x] CI/CD pipeline configured
- [x] NuGet package metadata complete
- [x] Epic 1.8 completed - Target exceeded!
- [x] **Epic 1.9 completed - 95%+ coverage achieved!** 🎉

---

**Instructions:**
- Use checkboxes to track progress.
- Update this file as you add tests and improve coverage.
- Add new subtasks if new uncovered areas are discovered.
