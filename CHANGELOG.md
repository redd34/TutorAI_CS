# Changelog

All notable changes to the AI Tutor System project will be documented in this file.

## [Unreleased]

### Added - AI Module Implementation (Task 4)

#### Task 4.1: Core Interfaces and Models
- Created `IAIProvider` interface with methods for text generation, streaming, and embedding generation
- Added `AIRequest` model with system/user prompts, temperature, max tokens, and additional parameters
- Added `AIResponse` model with content, token usage, model info, and success/error status
- Added `EmbeddingRequest` model for vector embedding generation requests
- Added `EmbeddingResponse` model for embedding results with vector data

#### Task 4.2: OpenRouter Provider Implementation
- Implemented `OpenRouterProvider` class supporting OpenRouter API integration
- Added text generation via `/chat/completions` endpoint
- Implemented streaming responses with real-time chunk delivery
- Added embedding generation via `/embeddings` endpoint
- Implemented comprehensive error handling with descriptive error messages
- Added internal response models for JSON deserialization (OpenRouterResponse, Choice, Message, Usage, etc.)
- Configured default embedding model: `openai/text-embedding-3-small`

#### Task 4.4: Claude Provider Implementation
- Implemented `ClaudeProvider` class supporting Anthropic Claude API integration
- Added text generation via `/messages` endpoint with Claude-specific format
- Implemented streaming responses with Server-Sent Events (SSE) parsing
- Added proper handling of Claude's content block structure
- Implemented graceful error response for embedding requests (Claude doesn't support embeddings natively)
- Added internal response models for JSON deserialization (ClaudeResponse, ContentBlock, ClaudeUsage, etc.)
- Configured API version: `2023-06-01`

#### Task 4.7: AI Factory Implementation
- Created `AIFactory` static factory class for configuration-based provider instantiation
- Implemented `Create` method with support for OpenRouter and Claude providers
- Added comprehensive configuration validation with descriptive error messages
- Implemented provider-specific factory methods: `CreateOpenRouterProvider` and `CreateClaudeProvider`
- Added XML documentation with configuration structure examples

#### Logging Enhancement (Post-Task 4)
- Added `Microsoft.Extensions.Logging.Abstractions` v9.0.0 dependency
- Integrated comprehensive logging throughout both AI providers
- **OpenRouterProvider Logging**:
  - Operation ID generation for request tracking (8-character GUID)
  - Start/completion logs with model, temperature, and max tokens
  - Token usage tracking (prompt tokens, completion tokens, total tokens)
  - Response metrics (duration, response length)
  - Error logging with operation context
  - Streaming metrics (total chunks, duration)
  - Embedding metrics (dimensions, token usage)
- **ClaudeProvider Logging**:
  - Operation ID generation for request tracking
  - Request ID tracking from Claude API responses
  - Token usage breakdown (input tokens, output tokens, total)
  - Stop reason tracking for completion analysis
  - Response metrics (duration, response length)
  - Error logging with operation context
  - Streaming metrics (total chunks, duration)
  - Warning logs for unsupported embedding requests
- **AIFactory Logging**:
  - Optional `ILoggerFactory` parameter for logger injection
  - Automatic logger creation for provider instances
  - Backward compatible (logger is optional)

### Changed
- Updated `AITutorSystem.AI.csproj` to include:
  - `Microsoft.Extensions.Configuration.Abstractions` v9.0.0 dependency
  - `Microsoft.Extensions.Logging.Abstractions` v9.0.0 dependency
- Removed placeholder `Class1.cs` file from AI module
- Enhanced `OpenRouterProvider` constructor to accept optional `ILogger<OpenRouterProvider>`
- Enhanced `ClaudeProvider` constructor to accept optional `ILogger<ClaudeProvider>`
- Updated `AIFactory.Create` method signature to accept optional `ILoggerFactory`

### Technical Details

#### Architecture Decisions
- **Provider Abstraction**: All AI providers implement `IAIProvider` interface for seamless switching
- **Error Handling**: Providers return error responses rather than throwing exceptions for API failures
- **Streaming Support**: Both providers support real-time streaming via `IAsyncEnumerable<string>`
- **Configuration-Based**: Factory pattern enables runtime provider selection via configuration
- **Embedding Limitation**: Claude provider returns descriptive error for embedding requests (not supported)
- **Optional Logging**: Logging is optional and doesn't break existing code without logger injection

#### Logging Metrics Tracked
- **Operation ID**: Unique 8-character identifier for each API call
- **Request ID**: Provider-specific request identifiers (Claude)
- **Token Usage**: Prompt tokens, completion tokens, total tokens
- **Performance**: Duration in milliseconds for all operations
- **Response Metrics**: Content length, chunk counts for streaming
- **Error Context**: Full error details with operation correlation
- **Model Information**: Actual model used by provider
- **Embedding Dimensions**: Vector size for embedding operations

#### Code Quality
- All public APIs documented with XML comments
- Nullable reference types enabled throughout
- Async/await pattern used for all I/O operations
- SOLID principles followed (Single Responsibility, Open/Closed, Dependency Inversion)
- No dependencies on business logic modules (foundation module)
- Logging is non-intrusive and optional

#### Configuration Structure
```json
{
  "AIProvider": "OpenRouter",  // or "Claude"
  "OpenRouter": {
    "ApiKey": "your-api-key",
    "Model": "anthropic/claude-3.5-sonnet",
    "EmbeddingModel": "openai/text-embedding-3-small"
  },
  "Claude": {
    "ApiKey": "your-api-key",
    "Model": "claude-3-5-sonnet-20241022"
  }
}
```

### Requirements Validated
- **Requirement 10.1**: AI Module provides consistent interface for AI operations ✓
- **Requirement 10.2**: OpenRouter provider support ✓
- **Requirement 10.3**: Claude provider support ✓
- **Requirement 10.4**: Configuration-based provider selection ✓
- **Requirement 10.5**: Descriptive errors without exposing provider details ✓
- **Requirement 10.6**: Streaming response support ✓
- **Requirement 14.1**: Comprehensive error logging with context ✓

### Next Steps
- Task 4.3: Write unit tests for OpenRouterProvider (optional)
- Task 4.5: Write property test for AI provider configuration selection (optional)
- Task 4.6: Write property test for streaming response delivery (optional)
- Task 5: Implement Vector DB Module
