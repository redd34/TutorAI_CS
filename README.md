# AI Tutor System

A modular, serverless C# application that provides structured learning through predefined curricula with AI-powered content recommendations and assessments.

## Project Structure

```
AITutorSystem/
├── src/
│   ├── AITutorSystem.Memory/          # Storage abstraction layer
│   ├── AITutorSystem.VectorDB/         # Vector database abstraction
│   ├── AITutorSystem.AI/               # AI provider abstraction
│   ├── AITutorSystem.Core/             # Business logic
│   ├── AITutorSystem.Lambda/           # AWS Lambda handlers
│   └── AITutorSystem.Frontend/         # Chat interface
├── tests/
├── appsettings.json                    # Local development configuration
├── appsettings.Lambda.json             # AWS Lambda configuration
└── AITutorSystem.sln                   # Solution file
```

## Modules

### Foundation Modules
- **Memory**: Storage abstraction supporting both local file system and AWS (S3/DynamoDB)
- **VectorDB**: Vector database abstraction for semantic search (local, Pinecone, Weaviate)
- **AI**: AI provider abstraction supporting OpenRouter and Claude

### Core Modules
- **Core**: Business logic including Quiz, Suggestion Engine, Indexing Worker, and Learning Path services
- **Lambda**: AWS Lambda function handlers for serverless deployment
- **Frontend**: Chat-based user interface

## Configuration

### Local Development
Edit `appsettings.json` to configure:
- AI provider (OpenRouter or Claude) and API keys
- Vector store provider
- Local storage path
- YouTube API key for content indexing

### AWS Lambda Deployment
Edit `appsettings.Lambda.json` to configure:
- S3 bucket and DynamoDB table names
- Cloud vector database settings (Pinecone or Weaviate)
- AI provider settings

## Getting Started

### Prerequisites
- .NET 10.0 SDK or later
- (Optional) AWS account for Lambda deployment
- (Optional) API keys for AI providers and vector databases

### Build
```bash
dotnet build
```

### Run Locally
```bash
cd src/AITutorSystem.Frontend
dotnet run
```

### Deploy to AWS Lambda
(Deployment instructions will be added after Lambda handlers are implemented)

## Architecture

The system follows SOLID principles with clear separation of concerns:
- Foundation modules have no dependencies on business logic
- Business logic depends on abstractions, not concrete implementations
- Environment-based configuration enables seamless switching between local and cloud execution

## Learning Flow

1. Student selects a learning path (e.g., Python)
2. Baseline assessment establishes current knowledge level
3. Personalized learning plan is created
4. Student receives content recommendations for current task
5. Progress quiz verifies skill acquisition
6. Profile updates and cycle continues, or alternative materials are suggested if quiz fails

## License

(Add your license here)
