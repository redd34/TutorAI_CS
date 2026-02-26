# AITutorSystem.AI.Tests

Unit tests for the AITutorSystem.AI module.

## Running Tests

```bash
# Run all tests
dotnet test

# Run tests with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run specific test class
dotnet test --filter "FullyQualifiedName~OpenRouterProviderTests"
```

## Test Structure

- `OpenRouterProviderTests.cs` - Unit tests for OpenRouterProvider
  - Constructor validation tests
  - GenerateAsync tests (success and error scenarios)
  - GenerateEmbeddingAsync tests
  - StreamAsync tests

## Integration Testing (Optional)

The current tests use mocked HTTP responses. To add integration tests with real API calls:

1. Create a file `appsettings.Test.json` in this directory (already in .gitignore):

```json
{
  "OpenRouter": {
    "ApiKey": "YOUR_API_KEY_HERE",
    "Model": "openai/gpt-4",
    "EmbeddingModel": "openai/text-embedding-3-small"
  }
}
```

2. Create a new test class `OpenRouterProviderIntegrationTests.cs` with the `[Fact(Skip = "Integration test")]` attribute
3. Load configuration from `appsettings.Test.json` and create real HttpClient instances
4. Remove the Skip attribute when you want to run integration tests locally

## Notes

- Unit tests use Moq to mock HttpClient behavior
- Tests validate both success and error scenarios
- No real API calls are made during unit tests
- Integration tests should be run manually with valid API keys
