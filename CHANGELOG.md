# Changelog

All notable changes to this project will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-10-03

### 🎉 Initial Production Release

This is the first production-ready release of ResilientHttpClient, a drop-in replacement for HttpClient with built-in resilience patterns.

### Added

#### ResilientHttpClient.Core v1.0.0
- ✅ **Circuit Breaker Pattern** - Prevents cascading failures by stopping requests after consecutive failures
- ✅ **Retry Logic** - Automatically retries transient failures (5xx, timeouts, network errors)
- ✅ **Timeout Handling** - Treats timeouts as transient failures eligible for retry
- ✅ **Per-Request Policies** - Customize retry/circuit breaker behavior for individual requests via fluent API
- ✅ **Content Cloning** - Properly clones HttpContent during retries for POST/PUT requests
- ✅ **Thread-Safe** - Safe for concurrent use across multiple threads
- ✅ **Unity Compatible** - .NET Standard 2.1 with no external dependencies
- ✅ **Drop-in Replacement** - Implements the same interface as HttpClient
- ✅ **96.7% Code Coverage** - 112 comprehensive unit tests

#### ResilientHttpClient.Extensions.DependencyInjection v1.0.0
- ✅ **Simple Registration** - `services.AddResilientHttpClient()` for basic setup
- ✅ **Named Clients** - Register multiple clients with `AddNamedResilientHttpClient()`
- ✅ **Configuration Support** - Configure options via actions or IOptions pattern
- ✅ **Lifetime Options** - Support for Singleton, Scoped, and Transient lifetimes
- ✅ **Factory Pattern** - `IResilientHttpClientFactory` for resolving named clients
- ✅ **25 Unit Tests** - Comprehensive test coverage for all DI scenarios

### Fixed
- 🐛 **Critical: Content Cloning Bug** - POST/PUT requests now properly clone content during retries
  - Previously, content was shared between retry attempts, causing "Cannot access a disposed object" errors
  - Now uses async content cloning with full header preservation
  - Improved request disposal logic to prevent premature cleanup

### Documentation
- 📚 **Best Practices Guide** - Critical guidance on instance reuse to avoid socket exhaustion
- 📚 **Architecture Decisions** - ARCHITECTURE.md explaining design choices and Unity compatibility
- 📚 **XML Documentation** - Comprehensive inline documentation with examples
- 📚 **DI Integration Guide** - Full documentation for ASP.NET Core integration
- 📚 **README** - Complete usage examples, quick start, and API reference

### Technical Details
- **Target Framework**: .NET Standard 2.1
- **Dependencies**: 
  - Core: System.Net.Http 4.3.4
  - DI Extension: Microsoft.Extensions.DependencyInjection.Abstractions 2.1.0, Microsoft.Extensions.Options 2.1.0
- **Total Tests**: 137 (all passing)
- **Code Coverage**: 96.7% line coverage

---

## [Unreleased]

### Planned for v1.1
- ⏳ Exponential backoff retry strategy
- ⏳ Configuration binding from appsettings.json
- ⏳ Health checks integration
- ⏳ Typed clients support
- ⏳ Custom HttpMessageHandler support

---

[1.0.0]: https://github.com/markcastle/ResilientHttpClient/releases/tag/v1.0.0
