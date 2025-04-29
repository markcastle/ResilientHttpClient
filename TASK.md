# TASK.md – Code Coverage Improvement Roadmap

## Epic 1: Achieve 100% Line and Branch Code Coverage

### Why?
Improving code coverage ensures reliability, maintainability, and confidence in the ResilientHttpClient library. This epic will guide us step-by-step toward 100% test coverage.

---

## Epic 1.1: Audit Current Coverage
- [ ] Review the latest coverage report and list all uncovered files, classes, and methods.
- [ ] Identify critical logic and branches that are not tested.

## Epic 1.2: Cover Core Client Logic
- [ ] Write tests for all uncovered methods in `ResilientHttpClient`
- [ ] Ensure all retry, timeout, and circuit breaker paths are tested (success, failure, edge cases)
- [ ] Cover all exception and error handling logic
- [ ] Test all configuration options (timeouts, retry counts, etc.)

## Epic 1.3: Cover Extension Methods
- [ ] Audit and test all extension methods in `HttpRequestMessageExtensions`
- [ ] Add tests for edge cases (nulls, invalid input, etc.)

## Epic 1.4: Cover Policy and Builder Logic
- [ ] Write tests for `RequestPolicy`, `RequestPolicyBuilder`, and related classes
- [ ] Ensure all policy configuration branches are exercised

## Epic 1.5: Cover Factory and Options Classes
- [ ] Write tests for `ResilientHttpClientFactory` and `ResilientHttpClientOptions`
- [ ] Ensure all public methods and properties are tested

## Epic 1.6: Increase Branch Coverage
- [ ] Review all `if`, `else`, and `switch` statements across the codebase
- [ ] Ensure both true/false and all case branches are tested
- [ ] Add tests to cover exception paths and rare error conditions

## Epic 1.7: Continuous Validation
- [ ] Rerun coverage after each round of new tests
- [ ] Update this checklist as coverage improves
- [ ] Mark each subtask as complete when coverage for that area reaches 100%

---

## Discovered During Work
- [ ] (Add any new tasks or edge cases found during test writing here)

---

## Progress Tracking
- [ ] 100% line and branch coverage achieved (final validation)

---

**Instructions:**
- Use checkboxes to track progress.
- Update this file as you add tests and improve coverage.
- Add new subtasks if new uncovered areas are discovered.
