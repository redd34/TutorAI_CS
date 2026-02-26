# Changelog

All notable changes to this project will be documented in this file.

## [Unreleased]

### Added
- Created test project `AITutorSystem.AI.Tests` for unit testing the AI module
- Added comprehensive unit tests for `OpenRouterProvider`:
  - Constructor validation tests (null parameter checks)
  - `GenerateAsync` tests for success and error scenarios
  - `GenerateEmbeddingAsync` tests for embeddings API
  - `StreamAsync` tests for streaming responses
- Added integration test template (`OpenRouterProviderIntegrationTests.cs`) for manual testing with real API keys
- Added test project README with instructions for running tests and adding API keys
- Updated `.gitignore` to exclude test configuration files (`appsettings.Test.json`)

### Technical Details
- Test framework: xUnit 2.9.2
- Mocking library: Moq 4.20.72
- All unit tests use mocked HTTP responses (no real API calls)
- Integration tests are skipped by default and require manual API key configuration
- 9 unit tests passing, 4 integration tests skipped by default
