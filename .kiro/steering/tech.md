# Technology Stack

## Framework & Language

- **.NET 10.0** - Target framework for all projects
- **C#** - Primary language with nullable reference types enabled
- **Implicit usings** enabled across all projects

## Project Structure

The solution uses a modular architecture with separate C# class library projects:

- **Foundation Modules** (no business logic dependencies):
  - `AITutorSystem.Memory` - Storage abstraction layer
  - `AITutorSystem.VectorDB` - Vector database abstraction
  - `AITutorSystem.AI` - AI provider abstraction

- **Core Modules**:
  - `AITutorSystem.Core` - Business logic (Quiz, Suggestion Engine, Indexing Worker)
  - `AITutorSystem.Lambda` - AWS Lambda function handlers
  - `AITutorSystem.Frontend` - ASP.NET Core web application (chat interface)

## Key Dependencies

- **AWS SDK**: `AWSSDK.S3`, `AWSSDK.DynamoDBv2` (v3.7.400+)
- **Microsoft.Extensions.Configuration.Abstractions** (v9.0.0+)
- Configuration-based dependency injection

## Build System

Solution file: `AITutorSystem.slnx` (XML-based solution format)

### Common Commands

```bash
# Build entire solution
dotnet build

# Build specific project
dotnet build src/AITutorSystem.Memory/AITutorSystem.Memory.csproj

# Run frontend locally
cd src/AITutorSystem.Frontend
dotnet run

# Run tests (when test projects exist)
dotnet test
```

## Configuration

- **Local**: `appsettings.json` - Local development settings
- **Lambda**: `appsettings.Lambda.json` - AWS deployment settings
- Environment-based provider selection via `Environment` configuration key

## AI & Vector Database Support

- **AI Providers**: OpenRouter, Claude (abstracted via AI module)
- **Vector Databases**: Local in-memory, Pinecone, Weaviate (abstracted via VectorDB module)
- **Storage**: Local file system or AWS (S3 + DynamoDB)
