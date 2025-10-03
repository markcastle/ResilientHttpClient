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

## Epic 2.0: Simple Console Demo 🎯

**Goal**: Create a simple console app showing the library in action

### Setup
- [x] Create new console project: `ResilientHttpClient.Demo`
- [x] Add reference to ResilientHttpClient.Core
- [x] Add to solution file

### Demo Examples (Keep it Simple)
- [x] **Example 1: Basic GET request**
  - Create client with factory
  - Make simple GET request
  - Print response
  
- [x] **Example 2: Show retry in action**
  - Use JSONPlaceholder API (or similar free API)
  - Configure retry settings
  - Display when retries happen
  
- [x] **Example 3: Custom policy**
  - Show how to use WithPolicy()
  - Custom retry count and delay
  
- [x] **Example 4: Circuit breaker demo**
  - Simple demo of circuit opening/closing
  - Clear console output showing state changes

### Output
- [x] Clean console output (no fancy UI needed)
- [x] Comments in code explaining what's happening
- [x] README.md in demo folder with run instructions

---

## Progress Tracking - Epic 2.0
- [x] Console demo created ✅
- [x] Examples working (4 examples complete) ✅
- [x] Demo README added ✅
- [x] Main README updated with demo section ✅
- [x] **Epic 2.0 Complete!** 🎉

---

## Epic 2.1: Critical Fixes Before v1.0 🔧

**Goal**: Address critical design issues identified in code review while maintaining HttpClient compatibility

### Critical Fix: Request Content Cloning
**Problem**: Content is shared, not cloned during retries. POST/PUT with non-seekable content will fail on retry.

- [x] **Fix CloneHttpRequestMessage to clone content** ✅
  - [x] Changed to async method to properly read content
  - [x] Read content into memory before cloning
  - [x] Create new ByteArrayContent for clone
  - [x] Copy all content headers to cloned content
  - [x] Removed `using` statements from wrapper methods (causing premature disposal)
  - [x] Added proper disposal logic for cloned requests
  - [x] Test: POST request with StringContent that gets retried
  - [x] Test: POST request with JSON content and headers
  - [x] Test: PUT request with ByteArrayContent that gets retried
  - [x] Test: PUT request with custom content headers
  - [x] Test: GET request without content (should not break)
  - [x] Test: SendAsync with manual request creation
  - [x] Test: Multiple retries with content cloning
  - [x] Test: Empty content handling
  
### Critical Documentation: HttpClient Instance Reuse
**Problem**: Factory creates new HttpClient instances (socket exhaustion anti-pattern)

- [x] **Add "Best Practices" section to README** ✅
  - [x] ⚠️ Warning about creating ONE instance per application
  - [x] Show singleton pattern example
  - [x] Explain socket exhaustion issue
  - [x] Code example for Unity: static readonly field
  - [x] Code example for ASP.NET Core DI: AddSingleton
  - [x] Code example for console apps
  - [x] Anti-pattern examples (what NOT to do)
  - [x] Thread safety explanation
  
- [x] **Update Quickstart section** ✅
  - [x] Add comment showing reuse pattern
  - [x] Link to Best Practices section

### Documentation: Design Decisions
**Problem**: Some design choices need to be documented as intentional for Unity compatibility

- [x] **Add "Architecture Decisions" document (ARCHITECTURE.md)** ✅
  - [x] Per-instance circuit breaker (explain why)
  - [x] HttpRequestMessage.Properties usage (.NET Standard 2.1)
  - [x] Static retry delay (exponential backoff planned for v1.1)
  - [x] Large interface mirroring HttpClient (intentional)
  - [x] Request disposal semantics for content cloning
  
- [x] **Add XML doc comments to ResilientHttpClientFactory** ✅
  - [x] Document that HttpClient instances are created fresh
  - [x] Recommend singleton usage pattern
  - [x] Add remarks about socket exhaustion
  - [x] Include code examples in XML documentation

### Optional: Thread Safety Improvements
- [ ] **Review thread safety in circuit breaker**
  - Consider locking _circuitOpenTime reads in IsCircuitOpen()
  - Add concurrent access tests
  - Document thread safety guarantees

---

## Progress Tracking - Epic 2.1
- [x] Content cloning fixed ✅ (96.7% line coverage!)
- [x] 8 comprehensive tests added (all passing) ✅
- [x] Best Practices documentation added ✅
- [x] XML documentation updated ✅
- [x] Architecture decisions documented ✅ (ARCHITECTURE.md)
- [x] Thread safety documented ✅
- [x] All changes tested and validated ✅
- [x] **Epic 2.1 Complete!** 🎉

### Summary of Epic 2.1 Achievements
- **Fixed critical content cloning bug** - POST/PUT requests now properly clone content during retries
- **Improved coverage** - 96.7% line coverage (up from 95.2%)
- **Comprehensive documentation** - Best Practices section with Unity examples
- **Architecture clarity** - ARCHITECTURE.md explains all design decisions
- **Production ready** - All critical v1.0 issues resolved

---

## Epic 2.5: Microsoft Dependency Injection Integration 🔌

**Goal**: Create a dedicated package (`ResilientHttpClient.Extensions.DependencyInjection`) that provides seamless integration with Microsoft.Extensions.DependencyInjection, making it easy to use in ASP.NET Core and other .NET applications.

### Problem Statement
Currently, users need to manually register `ResilientHttpClient` in DI containers:
```csharp
// Current approach - manual registration
services.AddSingleton<IResilientHttpClient>(sp => 
    ResilientHttpClientFactory.CreateClient("https://api.example.com", options));
```

This is error-prone and doesn't follow .NET conventions. We need a fluent API similar to `IHttpClientFactory`.

### Proposed Solution
Create an extension package that provides:
```csharp
// Desired API - fluent and idiomatic
services.AddResilientHttpClient("MyApiClient", options => 
{
    options.BaseAddress = "https://api.example.com";
    options.MaxRetries = 3;
    options.CircuitResetTime = TimeSpan.FromSeconds(30);
});
```

### Implementation Tasks

#### Phase 1: Project Setup
- [x] **Create new project: ResilientHttpClient.Extensions.DependencyInjection** ✅
  - [x] Target: .NET Standard 2.1 (for Unity compatibility)
  - [x] Add reference to ResilientHttpClient.Core
  - [x] Add NuGet: Microsoft.Extensions.DependencyInjection.Abstractions (2.1.0)
  - [x] Add NuGet: Microsoft.Extensions.Options (2.1.0)
  - [x] Added to solution
  
#### Phase 2: Core Extension Methods
- [x] **Create ServiceCollectionExtensions class** ✅
  - [x] `AddResilientHttpClient()` - default options
  - [x] `AddResilientHttpClient(Action<ResilientHttpClientOptions> configure, ServiceLifetime lifetime)` - with options
  - [x] `AddResilientHttpClient(string baseAddress, Action<ResilientHttpClientOptions> configure, ServiceLifetime lifetime)` - with base address
  - [x] `AddResilientHttpClient(ResilientHttpClientOptions options, ServiceLifetime lifetime)` - explicit options
  - [x] Register as Singleton by default
  - [x] Configurable lifetime support

#### Phase 3: Named Clients Support
- [x] **Implement IHttpClientFactory-style named clients** ✅
  ```csharp
  services.AddNamedResilientHttpClient("GitHub", "https://api.github.com", opt => { });
  services.AddNamedResilientHttpClient("MyAPI", "https://myapi.com");
  
  // Inject and resolve by name
  public class MyService
  {
      private readonly IResilientHttpClient _githubClient;
      
      public MyService(IResilientHttpClientFactory factory)
      {
          _githubClient = factory.CreateClient("GitHub");
      }
  }
  ```
  - [x] Created `IResilientHttpClientFactory` interface
  - [x] Implemented factory with named client resolution
  - [x] Created NamedClientExtensions with `AddNamedResilientHttpClient()` methods
  - [x] Factory automatically registered as singleton

#### Phase 4: Options Pattern Integration
- [ ] **Integrate with IOptions<T> pattern**
  ```csharp
  // appsettings.json
  {
    "ResilientHttpClient": {
      "MaxRetries": 5,
      "CircuitResetTime": "00:00:45"
    }
  }
  
  // Startup.cs
  services.Configure<ResilientHttpClientOptions>(
      Configuration.GetSection("ResilientHttpClient"));
  services.AddResilientHttpClient();
  ```
  - Support IOptions<ResilientHttpClientOptions>
  - Support IOptionsSnapshot for runtime updates
  - Validate options on registration

#### Phase 5: Advanced Features
- [ ] **Health checks integration**
  - Expose circuit breaker state for health checks
  - Implement IHealthCheck for monitoring
  
- [ ] **Typed clients support**
  ```csharp
  services.AddResilientHttpClient<IGitHubApiClient, GitHubApiClient>(opt => 
      opt.BaseAddress = "https://api.github.com");
  ```
  
- [ ] **HttpMessageHandler integration**
  - Allow custom message handlers in the pipeline
  - Support delegating handlers for logging, auth, etc.

#### Phase 6: Testing
- [x] **Create comprehensive unit tests** ✅
  - [x] Test service registration (multiple overloads)
  - [x] Test options configuration
  - [x] Test named client resolution
  - [x] Test singleton/scoped/transient lifecycle
  - [x] Test options validation (null checks)
  - [x] Test with actual DI container (ServiceCollection)
  - [x] Test multiple named clients
  - [x] Test unknown client name throws exception
  - [x] 25 tests created, all passing ✅
  
- [ ] **Create integration tests** (Optional - basic tests done)
  - [ ] Test with ASP.NET Core WebApplicationBuilder
  - [ ] Test configuration binding from appsettings.json

#### Phase 7: Documentation
- [x] **Update README.md** ✅
  - [x] Added "Dependency Injection" section
  - [x] Show ASP.NET Core examples
  - [x] Show basic and advanced registration patterns
  - [x] Document named clients pattern with full examples
  - [x] Updated Features section
  - [x] Updated Recent Improvements section
  
- [ ] **Create samples project**
  - ASP.NET Core Web API example
  - Console app with Generic Host example
  - Named clients example
  - Typed clients example
  
- [ ] **Update ARCHITECTURE.md**
  - Document DI integration design
  - Explain singleton vs transient choices
  - Document named clients implementation

#### Phase 8: NuGet Package
- [ ] **Prepare NuGet package metadata**
  - Package ID: ResilientHttpClient.Extensions.DependencyInjection
  - Description: Microsoft DI integration for ResilientHttpClient
  - Tags: httpclient, dependency-injection, di, aspnetcore, resilience
  - Add package icon
  
- [ ] **Create .nuspec or update .csproj**
  - Add package dependencies
  - Set version to match core library
  - Add README.md to package

### Design Decisions

**Q: Singleton or Scoped?**
**A:** Singleton by default (matches HttpClient best practices), but allow configuration:
```csharp
services.AddResilientHttpClient(opt => { }, lifetime: ServiceLifetime.Scoped);
```

**Q: New package or add to Core?**
**A:** New package - keeps core lightweight and Unity-compatible. Users who don't need DI won't have extra dependencies.

**Q: Support IHttpClientFactory pattern exactly?**
**A:** Similar but adapted. IHttpClientFactory manages lifecycle and handlers differently. We'll provide a familiar API but document differences.

### Success Criteria
- [ ] ASP.NET Core users can add ResilientHttpClient with 1-2 lines of code
- [ ] Supports configuration from appsettings.json
- [ ] Named clients work as expected
- [ ] 90%+ test coverage on DI package
- [ ] Documentation includes real-world examples
- [ ] Published to NuGet

### Estimated Effort
- **Phase 1-2:** 2-3 hours (basic implementation)
- **Phase 3-4:** 3-4 hours (named clients + options)
- **Phase 5:** 2-3 hours (advanced features - optional for v1.0)
- **Phase 6-7:** 3-4 hours (testing + docs)
- **Phase 8:** 1-2 hours (packaging)

**Total: ~11-16 hours for full implementation**

---

## Progress Tracking - Epic 2.5
- [x] Project created and dependencies added ✅
- [x] Basic extension methods implemented ✅
- [x] Named clients support added ✅
- [x] Tests written (25 tests, all passing!) ✅
- [x] Documentation updated (README.md) ✅
- [x] **Phase 1-3, 6-7 COMPLETE** ✅
- [ ] Options pattern integrated (basic support done, appsettings.json binding next)
- [ ] Advanced features completed (optional)
- [ ] Samples project created (optional)
- [x] NuGet package metadata complete ✅

### Current Test Summary
- **137 total tests passing** (112 core + 25 DI)
- Service registration tests: 16 tests ✅
- Named client tests: 9 tests ✅
- Validation tests: All edge cases covered ✅

---

## Epic 3: v1.0 Release Preparation 🚀

**Goal**: Prepare and publish v1.0.0 to NuGet with both Core and DI packages.

### Package Preparation
- [x] **Add NuGet metadata to DI package** ✅
  - [x] PackageId, Version (1.0.0), Description
  - [x] Authors, Tags, License
  - [x] Release notes
  - [x] README reference
  - [x] XML documentation generation

- [x] **Update Core package metadata** ✅
  - [x] Updated description (96.7% coverage, 112 tests)
  - [x] Updated release notes for v1.0.0
  - [x] Added content cloning bug fix details
  - [x] Mentioned DI extension package

### Documentation
- [x] **Update README badges** ✅
  - [x] Coverage: 96.7%
  - [x] Tests: 137 passing
  - [x] Version: 1.0.0

- [x] **Create CHANGELOG.md** ✅
  - [x] v1.0.0 release notes
  - [x] Feature list for both packages
  - [x] Bug fixes documented
  - [x] Planned features for v1.1

- [x] **Update DEPLOYMENT.md** ✅
  - [x] Instructions for both packages
  - [x] Individual package publishing
  - [x] Versioning best practices

### CI/CD
- [x] **Update GitHub Actions workflow** ✅
  - [x] Pack Core NuGet package
  - [x] Pack DI Extension package
  - [x] Push both to NuGet on tag

### Testing & Validation
- [x] **Build packages locally** ✅
  - [x] `dotnet build --configuration Release`
  - [x] `dotnet test --configuration Release`
  - [x] `dotnet pack --configuration Release`
  - [x] Verified both .nupkg files created

### Release Checklist
- [x] All 137 tests passing ✅
- [x] 96.7% code coverage maintained ✅
- [x] Both packages build successfully ✅
- [x] Documentation complete ✅
- [x] CI/CD configured ✅
- [ ] **Ready to tag and publish!** 🎯

### To Publish v1.0.0:
```bash
# 1. Commit all changes
git add .
git commit -m "Release v1.0.0"

# 2. Create and push tag
git tag v1.0.0
git push origin main
git push origin v1.0.0

# 3. GitHub Actions will automatically publish to NuGet
```

### Success Criteria
- ✅ Both packages available on NuGet.org
- ✅ README shows correct version and badges
- ✅ CHANGELOG.md tracks version history
- ✅ All documentation up to date

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
