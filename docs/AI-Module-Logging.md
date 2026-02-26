# AI Module Logging Guide

## Overview

The AI Module includes comprehensive logging to track API usage, performance metrics, and operational statistics. All logging is optional and uses `Microsoft.Extensions.Logging.Abstractions` for framework-agnostic integration.

## Logged Metrics

### Common Metrics (All Operations)
- **Operation ID**: Unique 8-character identifier for request correlation
- **Model**: AI model being used
- **Duration**: Operation duration in milliseconds
- **Success/Failure**: Operation outcome with error details

### Text Generation Metrics
- **Temperature**: Randomness parameter (0.0-1.0)
- **Max Tokens**: Maximum tokens to generate
- **Token Usage**:
  - Prompt Tokens (input)
  - Completion Tokens (output)
  - Total Tokens
- **Response Length**: Character count of generated text

### Streaming Metrics
- **Total Chunks**: Number of streaming chunks received
- **Duration**: Total streaming time

### Embedding Metrics
- **Text Length**: Input text character count
- **Embedding Dimensions**: Vector size (e.g., 1536 for text-embedding-3-small)
- **Token Usage**: Tokens consumed for embedding generation

## Example Log Output

### OpenRouter Text Generation (Success)

```
[Information] OpenRouter GenerateAsync started. OperationId: a3f7b2c1, Model: anthropic/claude-3.5-sonnet, Temperature: 0.7, MaxTokens: 1000

[Information] OpenRouter GenerateAsync completed. OperationId: a3f7b2c1, Model: anthropic/claude-3.5-sonnet, TokensUsed: 245, PromptTokens: 45, CompletionTokens: 200, Duration: 1523.45ms, ResponseLength: 892
```

### OpenRouter Text Generation (Error)

```
[Information] OpenRouter GenerateAsync started. OperationId: b8e4c9d2, Model: anthropic/claude-3.5-sonnet, Temperature: 0.7, MaxTokens: 1000

[Error] OpenRouter API error. OperationId: b8e4c9d2, StatusCode: 429, Response: {"error":{"message":"Rate limit exceeded","type":"rate_limit_error"}}
```

### Claude Text Generation (Success)

```
[Information] Claude GenerateAsync started. OperationId: c5d9a1f3, Model: claude-3-5-sonnet-20241022, Temperature: 0.7, MaxTokens: 1000

[Information] Claude GenerateAsync completed. OperationId: c5d9a1f3, Model: claude-3-5-sonnet-20241022, RequestId: req_01ABC123XYZ, InputTokens: 52, OutputTokens: 187, TotalTokens: 239, Duration: 1876.23ms, ResponseLength: 834, StopReason: end_turn
```

### Streaming Operation

```
[Information] OpenRouter StreamAsync started. OperationId: d7f2b4e5, Model: anthropic/claude-3.5-sonnet, Temperature: 0.7, MaxTokens: 1000

[Information] OpenRouter StreamAsync completed. OperationId: d7f2b4e5, Model: anthropic/claude-3.5-sonnet, TotalChunks: 47, Duration: 2341.67ms
```

### Embedding Generation

```
[Information] OpenRouter GenerateEmbeddingAsync started. OperationId: e9a3c6f7, Model: openai/text-embedding-3-small, TextLength: 342

[Information] OpenRouter GenerateEmbeddingAsync completed. OperationId: e9a3c6f7, Model: openai/text-embedding-3-small, TokensUsed: 89, EmbeddingDimensions: 1536, Duration: 456.78ms
```

### Claude Embedding Attempt (Unsupported)

```
[Warning] Claude embedding request attempted. OperationId: f1b5d8a9, TextLength: 342. Claude does not support embeddings - use OpenRouter or another provider.
```

## Integration Example

### Basic Setup (No Logging)

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var httpClient = new HttpClient();
var aiProvider = AIFactory.Create(configuration, httpClient);
```

### With Logging

```csharp
var configuration = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json")
    .Build();

var loggerFactory = LoggerFactory.Create(builder =>
{
    builder
        .AddConsole()
        .AddDebug()
        .SetMinimumLevel(LogLevel.Information);
});

var httpClient = new HttpClient();
var aiProvider = AIFactory.Create(configuration, httpClient, loggerFactory);
```

### With Dependency Injection

```csharp
services.AddHttpClient();
services.AddSingleton<IAIProvider>(sp =>
{
    var configuration = sp.GetRequiredService<IConfiguration>();
    var httpClient = sp.GetRequiredService<HttpClient>();
    var loggerFactory = sp.GetRequiredService<ILoggerFactory>();
    
    return AIFactory.Create(configuration, httpClient, loggerFactory);
});
```

## Log Levels

- **Information**: Normal operation start/completion, metrics
- **Warning**: Non-critical issues (e.g., Claude embedding attempts)
- **Error**: API errors, exceptions, failures

## Performance Monitoring

Use the logged metrics to monitor:

1. **Token Usage**: Track costs across different models
2. **Response Times**: Identify slow operations
3. **Error Rates**: Monitor API reliability
4. **Model Performance**: Compare different models
5. **Streaming Efficiency**: Analyze chunk delivery patterns

## Correlation

All operations within a single request share the same Operation ID, enabling:
- Request tracing across logs
- Performance analysis per operation
- Error debugging with full context
- Usage analytics and reporting

## Privacy & Security

The logging implementation:
- ✓ Does NOT log API keys
- ✓ Does NOT log full prompt content
- ✓ Does NOT log full response content
- ✓ Logs only metadata and metrics
- ✓ Logs error messages (may contain API error details)

For production environments, consider:
- Filtering sensitive data from error messages
- Using structured logging for better analysis
- Implementing log aggregation (e.g., CloudWatch, Application Insights)
- Setting appropriate log retention policies
