# Implementation Plan: AI Tutor System

## Overview

This implementation plan breaks down the AI Tutor System into discrete, incremental tasks following SOLID principles. The system will be built as modular C# projects supporting both local development and AWS Lambda deployment. The implementation follows a bottom-up approach, starting with foundation modules (Memory, Vector DB, AI) and building up to business logic (Quiz, Suggestion, Indexing) and finally the frontend.

## Tasks

- [x] 1. Set up project structure and solution
  - Create C# solution file
  - Create project folders for all modules (Memory, VectorDB, AI, Core, Lambda, Frontend)
  - Set up .gitignore for C# projects
  - Create shared configuration files (appsettings.json templates)
  - _Requirements: 11.1, 11.2, 11.3_

- [x] 2. Implement Memory Module (Storage Abstraction)
  - [x] 2.1 Create core interfaces and models
    - Define IStorageProvider interface with CRUD operations
    - Create data models (StudentProfile, LearningPath, IndexedMaterial, QuizResult, LearningPlan)
    - Implement model equality and serialization support
    - _Requirements: 9.1, 9.4_
  
  - [ ]* 2.2 Write property test for storage round-trip consistency
    - **Property 3: Storage Round-Trip Consistency**
    - **Validates: Requirements 1.4, 2.5, 6.4, 9.4**
  
  - [x] 2.3 Implement LocalStorageProvider
    - Create file-based storage using JSON serialization
    - Implement all IStorageProvider methods
    - Add directory management and file I/O
    - _Requirements: 9.2, 11.1_
  
  - [x] 2.4 Write unit tests for LocalStorageProvider
    - Test file creation, reading, updating, deletion
    - Test error handling for invalid paths
    - _Requirements: 9.2_
  
  - [x] 2.5 Implement CloudStorageProvider
    - Integrate AWS SDK for S3 and DynamoDB
    - Implement storage routing (small objects → DynamoDB, large → S3)
    - Add retry logic with exponential backoff
    - _Requirements: 9.3, 11.2, 14.3_
  
  - [ ]* 2.6 Write property test for error message abstraction
    - **Property 21: Error Message Abstraction**
    - **Validates: Requirements 9.5, 10.5, 14.2**
  
  - [x] 2.7 Create StorageFactory
    - Implement environment-based provider selection
    - Add configuration reading logic
    - _Requirements: 9.2, 9.3, 12.3_
  
  - [ ]* 2.8 Write property test for environment-based provider selection
    - **Property 20: Environment-Based Storage Provider Selection**
    - **Validates: Requirements 9.2, 9.3, 12.3**

- [ ] 3. Checkpoint - Verify Memory Module
  - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Implement AI Module (AI Provider Abstraction)
  - [x] 4.1 Create core interfaces and models
    - Define IAIProvider interface with Generate, Stream, and GenerateEmbedding methods
    - Create AIRequest, AIResponse, and EmbeddingRequest models
    - _Requirements: 10.1, 10.6_
  
  - [x] 4.2 Implement OpenRouterProvider
    - Integrate with OpenRouter API using HttpClient
    - Implement text generation and streaming
    - Implement embedding generation
    - Add error handling and response parsing
    - _Requirements: 10.2_
  
  - [ ] 4.3 Write unit tests for OpenRouterProvider
    - Test API request formatting
    - Test response parsing
    - Test error handling
    - _Requirements: 10.2_
  
  - [x] 4.4 Implement ClaudeProvider
    - Integrate with Claude API using HttpClient
    - Implement text generation and streaming
    - Implement embedding generation
    - Add error handling and response parsing
    - _Requirements: 10.3_
  
  - [ ]* 4.5 Write property test for AI provider configuration selection
    - **Property 22: AI Provider Configuration Selection**
    - **Validates: Requirements 10.4**
  
  - [ ]* 4.6 Write property test for streaming response delivery
    - **Property 23: AI Streaming Response Delivery**
    - **Validates: Requirements 10.6**
  
  - [x] 4.7 Create AIFactory
    - Implement configuration-based provider selection
    - Add API key and model configuration reading
    - _Requirements: 10.4_

- [ ] 5. Implement Vector DB Module (Vector Database Abstraction)
  - [ ] 5.1 Create core interfaces and models
    - Define IVectorStore interface with Upsert, Search, SearchByText methods
    - Create VectorDocument and SearchResult models
    - _Requirements: 4.1_
  
  - [ ] 5.2 Implement LocalVectorStore
    - Create in-memory vector storage using Dictionary
    - Implement cosine similarity calculation
    - Implement filtering by metadata
    - Add embedding generation integration
    - _Requirements: 4.2_
  
  - [ ]* 5.3 Write unit tests for LocalVectorStore
    - Test vector upsert and retrieval
    - Test cosine similarity calculation
    - Test metadata filtering
    - _Requirements: 4.2_
  
  - [ ] 5.4 Implement PineconeProvider
    - Integrate with Pinecone API using HttpClient
    - Implement vector upsert and search
    - Add metadata filtering support
    - _Requirements: 4.3_
  
  - [ ] 5.5 Implement WeaviateProvider
    - Integrate with Weaviate API using HttpClient
    - Implement vector upsert and search with GraphQL
    - Add metadata filtering support
    - _Requirements: 4.3_
  
  - [ ] 5.6 Create VectorStoreFactory
    - Implement environment-based provider selection
    - Add configuration reading for vector store settings
    - _Requirements: 4.2, 4.3_

- [ ] 6. Checkpoint - Verify Foundation Modules
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 7. Implement Learning Path Service
  - [ ] 7.1 Create ILearningPathService interface and implementation
    - Define methods for getting available paths and creating paths
    - Implement path retrieval from storage
    - Add predefined path initialization
    - _Requirements: 1.1, 1.5_
  
  - [ ]* 7.2 Write property test for learning path availability
    - **Property 1: Learning Path Availability**
    - **Validates: Requirements 1.1**
  
  - [ ] 7.3 Create sample learning paths
    - Define Python learning path with goals and tasks
    - Define JavaScript learning path with goals and tasks
    - Store predefined paths in configuration or seed data
    - _Requirements: 1.1, 1.5_

- [ ] 8. Implement Quiz Module
  - [ ] 8.1 Create IQuizService interface and core models
    - Define Quiz, QuizQuestion, QuizResult, KnowledgeGap models
    - Define IQuizService interface with baseline and progress quiz methods
    - Create IQuizAnalyzer interface for result analysis
    - _Requirements: 2.1, 4.1, 4.2_
  
  - [ ] 8.2 Implement QuizAnalyzer
    - Implement knowledge gap identification from quiz results
    - Implement weak topic extraction
    - Implement pass/fail determination logic
    - _Requirements: 2.2, 4.2_
  
  - [ ]* 8.3 Write property test for knowledge gap identification
    - **Property 5: Knowledge Gap Identification**
    - **Validates: Requirements 2.2, 6.4**
  
  - [ ]* 8.4 Write property test for quiz evaluation correctness
    - **Property 10: Quiz Evaluation Correctness**
    - **Validates: Requirements 4.2**
  
  - [ ] 8.5 Implement QuizService - Baseline Assessment
    - Implement GenerateBaselineAssessmentAsync using AI provider
    - Create prompt templates for baseline quiz generation
    - Implement quiz parsing from AI responses
    - _Requirements: 2.1_
  
  - [ ]* 8.6 Write property test for quiz generation coverage
    - **Property 4: Quiz Generation Coverage**
    - **Validates: Requirements 2.1, 4.1**
  
  - [ ] 8.7 Implement QuizService - Progress Quiz
    - Implement GenerateProgressQuizAsync for specific tasks
    - Create prompt templates for progress quiz generation
    - _Requirements: 4.1_
  
  - [ ] 8.8 Implement QuizService - Evaluation and Learning Plan
    - Implement EvaluateQuizAsync with QuizAnalyzer integration
    - Implement CreateLearningPlanAsync with task prioritization
    - Add profile update logic
    - _Requirements: 2.2, 2.3, 2.4, 4.3, 4.4, 4.5_
  
  - [ ]* 8.9 Write property test for learning plan task prioritization
    - **Property 6: Learning Plan Task Prioritization**
    - **Validates: Requirements 2.4**
  
  - [ ]* 8.10 Write property test for profile update consistency
    - **Property 11: Profile Update Consistency**
    - **Validates: Requirements 2.3, 4.3, 4.5**
  
  - [ ]* 8.11 Write property test for task progression sequence
    - **Property 12: Task Progression Sequence**
    - **Validates: Requirements 4.4**

- [ ] 9. Implement Indexing Worker
  - [ ] 9.1 Create IIndexingService interface and models
    - Define VideoMetadata and VideoAnalysis models
    - Define IIndexingService interface with channel and video indexing methods
    - _Requirements: 7.1, 7.2, 7.3_
  
  - [ ] 9.2 Implement YouTube API integration
    - Add YouTube Data API client
    - Implement channel video list fetching
    - Implement video metadata fetching
    - Implement transcript fetching (using youtube-transcript-api or similar)
    - _Requirements: 7.1_
  
  - [ ] 9.3 Implement video content analysis
    - Create AI prompts for topic extraction
    - Create AI prompts for teaching style classification
    - Implement video analysis using AI provider
    - Parse analysis results into structured data
    - _Requirements: 7.2, 7.3_
  
  - [ ]* 9.4 Write property test for video indexing metadata extraction
    - **Property 16: Video Indexing Metadata Extraction**
    - **Validates: Requirements 7.2, 7.3**
  
  - [ ] 9.5 Implement vector embedding creation
    - Create rich text representation from video metadata and transcript
    - Generate embeddings using AI provider
    - Create VectorDocument with metadata
    - Store in vector database
    - _Requirements: 4.4, 4.7_
  
  - [ ] 9.6 Implement IndexingService with error handling
    - Implement IndexChannelAsync with batch processing
    - Implement IndexVideoAsync with storage and vector DB integration
    - Add error logging and continuation logic
    - _Requirements: 7.1, 7.4, 7.5_
  
  - [ ]* 9.7 Write property test for channel video extraction completeness
    - **Property 15: Channel Video Extraction Completeness**
    - **Validates: Requirements 7.1**
  
  - [ ]* 9.8 Write property test for indexing error recovery
    - **Property 17: Indexing Error Recovery**
    - **Validates: Requirements 7.5, 14.5**

- [ ] 10. Checkpoint - Verify Core Services
  - Ensure all tests pass, ask the user if questions arise.

- [ ] 11. Implement Suggestion Engine with RAG
  - [ ] 11.1 Create ISuggestionService and IRAGService interfaces
    - Define ISuggestionService with GetSuggestions and GetRetrySuggestions methods
    - Define IRAGService with ReRankWithContext and GenerateExplanation methods
    - Create MaterialSuggestion model
    - _Requirements: 3.1, 3.4, 6.1_
  
  - [ ] 11.2 Implement RAGService
    - Implement ReRankWithContextAsync using vector store and AI
    - Implement GenerateExplanationAsync for suggestion reasoning
    - Add context retrieval from vector database
    - _Requirements: 3.7, 4.5_
  
  - [ ] 11.3 Implement SuggestionService - Traditional Search
    - Implement GetTraditionalSuggestionsAsync with keyword-based filtering
    - Implement material ranking using AI
    - _Requirements: 3.1, 3.2, 3.3_
  
  - [ ]* 11.4 Write property test for suggestion topic matching
    - **Property 7: Suggestion Topic Matching**
    - **Validates: Requirements 3.1, 3.2**
  
  - [ ]* 11.5 Write property test for suggestion ranking by relevance
    - **Property 8: Suggestion Ranking by Relevance**
    - **Validates: Requirements 3.3**
  
  - [ ] 11.6 Implement SuggestionService - RAG-based Search
    - Implement GetRAGSuggestionsAsync with semantic search
    - Build semantic query from student profile and task
    - Integrate vector store search with metadata filtering
    - Integrate RAG service for re-ranking
    - _Requirements: 3.6, 3.7, 4.5, 4.6_
  
  - [ ] 11.7 Implement retry suggestion logic
    - Implement GetRetrySuggestionsAsync with exclusion filtering
    - Add teaching style diversification
    - Add prerequisite suggestion for multiple failures
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5_
  
  - [ ]* 11.8 Write property test for retry suggestion exclusion
    - **Property 13: Retry Suggestion Exclusion**
    - **Validates: Requirements 6.2, 6.3**
  
  - [ ]* 11.9 Write property test for prerequisite suggestion after multiple failures
    - **Property 14: Prerequisite Suggestion After Multiple Failures**
    - **Validates: Requirements 6.5**
  
  - [ ]* 11.10 Write property test for suggestion metadata completeness
    - **Property 9: Suggestion Metadata Completeness**
    - **Validates: Requirements 3.4**

- [ ] 12. Implement Session Management and Orchestration
  - [ ] 12.1 Create session initialization service
    - Implement learning path selection logic
    - Implement student profile creation
    - Implement session state management
    - _Requirements: 1.2, 1.3, 1.4_
  
  - [ ]* 12.2 Write property test for session initialization completeness
    - **Property 2: Session Initialization Completeness**
    - **Validates: Requirements 1.2, 1.3, 1.5**
  
  - [ ] 12.3 Create orchestration service for learning flow
    - Implement baseline → plan → suggestions → quiz → progress cycle
    - Add state transition logic
    - Integrate all core services (Quiz, Suggestion, LearningPath)
    - _Requirements: 2.1, 2.4, 3.1, 4.1, 4.4_

- [ ] 13. Implement Frontend (Chat Interface)
  - [ ] 13.1 Create chat controller and routing logic
    - Implement message routing to appropriate services
    - Add request parsing and validation
    - _Requirements: 8.1_
  
  - [ ]* 13.2 Write property test for frontend message routing
    - **Property 18: Frontend Message Routing**
    - **Validates: Requirements 8.1**
  
  - [ ] 13.3 Implement quiz rendering
    - Create quiz question display formatting
    - Add answer option rendering
    - _Requirements: 8.2_
  
  - [ ] 13.4 Implement suggestion rendering
    - Create material suggestion list formatting
    - Add clickable video links
    - Add relevance score and reasoning display
    - _Requirements: 8.3_
  
  - [ ] 13.5 Implement progress display
    - Show current task and position in learning path
    - Display completed tasks and quiz history
    - _Requirements: 8.4_
  
  - [ ]* 13.6 Write property test for frontend rendering completeness
    - **Property 19: Frontend Rendering Completeness**
    - **Validates: Requirements 8.2, 8.3, 8.4**
  
  - [ ] 13.7 Add activity feedback UI
    - Implement loading indicators
    - Add processing status messages
    - _Requirements: 8.5_
  
  - [ ] 13.8 Create basic HTML/CSS chat interface
    - Build simple chat UI with message history
    - Add input field and send button
    - Style for readability (minimal aesthetic)
    - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5_

- [ ] 14. Implement AWS Lambda Handlers
  - [ ] 14.1 Create QuizHandler Lambda function
    - Implement Lambda function handler for quiz operations
    - Add request/response serialization
    - Integrate with QuizService
    - _Requirements: 11.2, 12.2_
  
  - [ ] 14.2 Create SuggestionHandler Lambda function
    - Implement Lambda function handler for suggestion operations
    - Add request/response serialization
    - Integrate with SuggestionService
    - _Requirements: 11.2, 12.2_
  
  - [ ] 14.3 Create IndexingHandler Lambda function
    - Implement Lambda function handler for indexing operations
    - Add request/response serialization
    - Integrate with IndexingService
    - _Requirements: 11.2, 12.2_
  
  - [ ] 14.4 Add API Gateway integration
    - Configure API Gateway routes for each Lambda
    - Add CORS configuration
    - Set up request/response mapping
    - _Requirements: 11.2, 12.2_

- [ ] 15. Implement Error Handling and Resilience
  - [ ] 15.1 Add retry logic with exponential backoff
    - Create ExecuteWithRetryAsync helper method
    - Implement transient error detection
    - Add configurable retry count and delay
    - _Requirements: 14.3_
  
  - [ ]* 15.2 Write property test for transient failure retry count
    - **Property 25: Transient Failure Retry Count**
    - **Validates: Requirements 14.3**
  
  - [ ] 15.3 Implement comprehensive error logging
    - Add structured logging with context
    - Implement environment-specific logging (Console for local, CloudWatch for Lambda)
    - Add error categorization (transient, permanent, validation)
    - _Requirements: 12.5, 14.1_
  
  - [ ]* 15.4 Write property test for error logging context completeness
    - **Property 24: Error Logging Context Completeness**
    - **Validates: Requirements 14.1**
  
  - [ ] 15.5 Add data integrity protection
    - Implement transaction-like behavior for profile updates
    - Add rollback logic on failure
    - _Requirements: 14.4_
  
  - [ ]* 15.6 Write property test for data integrity on failure
    - **Property 26: Data Integrity on Failure**
    - **Validates: Requirements 14.4**

- [ ] 16. Configuration and Deployment Setup
  - [ ] 16.1 Create configuration files
    - Create appsettings.json for local development
    - Create appsettings.Lambda.json for AWS deployment
    - Add environment variable configuration
    - Document all configuration options
    - _Requirements: 11.3, 12.3, 12.4_
  
  - [ ] 16.2 Create deployment scripts
    - Create script for local development setup
    - Create AWS SAM or CDK template for Lambda deployment
    - Add deployment documentation
    - _Requirements: 11.2, 12.2_
  
  - [ ] 16.3 Set up dependency injection
    - Configure DI container for all services
    - Add factory registration
    - Implement environment-based service resolution
    - _Requirements: 13.5_

- [ ] 17. Integration Testing
  - [ ]* 17.1 Write end-to-end learning flow test
    - Test complete flow: path selection → baseline → suggestions → progress quiz
    - Verify state transitions and data persistence
    - _Requirements: 1.1, 2.1, 3.1, 4.1_
  
  - [ ]* 17.2 Write local vs Lambda execution test
    - Verify system works in local mode without AWS
    - Verify system works in Lambda mode with AWS services
    - _Requirements: 11.1, 11.2, 12.1, 12.2_
  
  - [ ]* 17.3 Write provider switching test
    - Test switching between storage providers
    - Test switching between AI providers
    - Test switching between vector store providers
    - _Requirements: 9.2, 9.3, 10.2, 10.3, 4.2, 4.3_

- [ ] 18. Final Checkpoint and Documentation
  - Ensure all tests pass, ask the user if questions arise.
  - Verify all requirements are implemented
  - Review code for SOLID principles compliance
  - Update README with setup and usage instructions

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Each task references specific requirements for traceability
- Property tests validate universal correctness properties with minimum 100 iterations
- Unit tests validate specific examples and edge cases
- The implementation follows a bottom-up approach: foundation modules first, then business logic, then UI
- All modules support both local development and AWS Lambda deployment through abstraction
