# Project Structure & Conventions

## Directory Layout

```
AITutorSystem/
├── src/                              # Source code
│   ├── AITutorSystem.Memory/         # Storage abstraction (foundation)
│   │   ├── Interfaces/               # IStorageProvider interface
│   │   ├── Models/                   # Data models (StudentProfile, LearningPath, etc.)
│   │   ├── Providers/                # LocalStorageProvider, CloudStorageProvider
│   │   └── StorageFactory.cs         # Factory for provider creation
│   ├── AITutorSystem.VectorDB/       # Vector database abstraction (foundation)
│   ├── AITutorSystem.AI/             # AI provider abstraction (foundation)
│   ├── AITutorSystem.Core/           # Business logic
│   ├── AITutorSystem.Lambda/         # AWS Lambda handlers
│   └── AITutorSystem.Frontend/       # ASP.NET Core web app
├── tests/                            # Test projects (to be added)
├── .kiro/                            # Kiro configuration
│   ├── specs/                        # Feature specifications
│   └── steering/                     # Steering documents
├── appsettings.json                  # Local configuration
├── appsettings.Lambda.json           # Lambda configuration
└── AITutorSystem.slnx                # Solution file
```

## Architectural Patterns

### Dependency Flow
- Foundation modules (Memory, VectorDB, AI) have NO dependencies on other modules
- Core business logic depends on foundation module abstractions
- Lambda and Frontend depend on Core and foundation modules
- **Rule**: Dependencies flow inward; foundation modules never reference business logic

### Factory Pattern
- Use static factory classes for creating providers based on configuration
- Example: `StorageFactory.Create(IConfiguration)` returns appropriate `IStorageProvider`

### Interface Abstraction
- All external dependencies (storage, AI, vector DB) accessed via interfaces
- Enables seamless switching between local and cloud implementations
- Example: `IStorageProvider` abstracts file system vs. S3/DynamoDB

## Code Conventions

### Naming
- **Namespaces**: Match folder structure (e.g., `AITutorSystem.Memory.Models`)
- **Interfaces**: Prefix with `I` (e.g., `IStorageProvider`)
- **Models**: Descriptive nouns (e.g., `StudentProfile`, `LearningPath`)
- **Factories**: Suffix with `Factory` (e.g., `StorageFactory`)

### Documentation
- XML documentation comments (`///`) for all public APIs
- Include `<summary>`, `<param>`, `<returns>`, and `<exception>` tags
- Document purpose and behavior, not implementation details

### Models
- Use auto-properties with initializers for collections: `= new()`
- Implement `Equals()` and `GetHashCode()` for value comparison when needed
- Use nullable reference types appropriately (`?` for optional properties)

### Error Handling
- Throw `InvalidOperationException` for configuration errors with descriptive messages
- Use null-coalescing with throw expressions: `?? throw new InvalidOperationException(...)`
- Return `null` or `false` for expected failure cases (not found, operation failed)

### Async/Await
- All I/O operations are async (storage, AI calls, HTTP requests)
- Use `Task<T>` return types for async methods
- Suffix async methods with `Async` (e.g., `GetAsync`, `SaveAsync`)

## Configuration Keys

### Required Settings
- `Environment`: "local" or "lambda"
- `LocalStoragePath`: Path for local file storage (local mode)
- `S3Bucket`: S3 bucket name (lambda mode)
- `DynamoTable`: DynamoDB table name (lambda mode)
