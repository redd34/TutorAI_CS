# Requirements Document: AI Tutor System

## Introduction

The AI Tutor System is a serverless application that provides structured, personalized learning through predefined learning paths. Students select a subject (e.g., Python), complete a baseline assessment, receive personalized content recommendations, and progress through iterative learning cycles with skill verification. The system runs on AWS Lambda with C# and supports both cloud deployment and local development execution.

## Glossary

- **Tutor_System**: The complete AI-based tutoring application
- **Quiz_Module**: Component responsible for creating quizzes and analyzing student progress
- **Suggestion_Engine**: Component that recommends learning materials based on student profile and current task
- **Indexing_Worker**: Component that processes YouTube videos to extract educational metadata
- **Memory_Module**: Abstraction layer for storage operations supporting both local and cloud storage
- **AI_Module**: Abstraction layer for AI provider interactions including text generation and embeddings
- **Vector_DB_Module**: Abstraction layer for vector database operations supporting semantic search
- **RAG_Service**: Service that uses Retrieval-Augmented Generation for context-aware recommendations
- **Student_Profile**: Comprehensive record of a student's knowledge level, completed tasks, and skill assessments
- **Learning_Path**: A predefined curriculum for a subject (e.g., Python) with goals and ordered tasks
- **Baseline_Assessment**: Initial quiz to determine student's starting knowledge level
- **Progress_Quiz**: Assessment to verify skill acquisition after completing learning activities
- **Task_List**: Ordered sequence of learning objectives within a Learning_Path
- **Learning_Plan**: Personalized sequence of tasks based on baseline assessment and learning path goals
- **Indexed_Material**: Educational content that has been processed and catalogued
- **Frontend**: Chat-based user interface for system interaction
- **Local_Wrapper**: Development-time storage implementation using local file system
- **Cloud_Storage**: Production storage implementation for AWS Lambda deployment

## Requirements

### Requirement 1: Learning Path Selection and Initialization

**User Story:** As a student, I want to select a learning path, so that I can start a structured course on a specific subject.

#### Acceptance Criteria

1. WHEN a student requests available subjects, THE Tutor_System SHALL return a list of predefined Learning_Paths
2. WHEN a student selects a Learning_Path, THE Tutor_System SHALL initialize a new learning session
3. WHEN a learning session is initialized, THE Tutor_System SHALL create a Student_Profile for tracking progress
4. THE Tutor_System SHALL persist the Student_Profile immediately after creation
5. WHEN a Learning_Path is selected, THE Tutor_System SHALL load the predefined goals and Task_List for that path

### Requirement 2: Baseline Assessment

**User Story:** As a student, I want to take an initial assessment, so that the system understands my current knowledge level.

#### Acceptance Criteria

1. WHEN a learning session starts, THE Quiz_Module SHALL generate a Baseline_Assessment covering key concepts from the Learning_Path
2. WHEN a student completes the Baseline_Assessment, THE Quiz_Module SHALL analyze results to identify strengths and knowledge gaps
3. WHEN baseline analysis is complete, THE Quiz_Module SHALL update the Student_Profile with the initial knowledge level
4. WHEN baseline analysis is complete, THE Quiz_Module SHALL create a Learning_Plan prioritizing tasks based on identified gaps
5. THE Quiz_Module SHALL persist the Baseline_Assessment results and Learning_Plan to storage

### Requirement 3: Personalized Content Recommendations

**User Story:** As a student, I want to receive relevant learning materials for my current task, so that I can learn effectively.

#### Acceptance Criteria

1. WHEN a student begins a task, THE Suggestion_Engine SHALL scan Indexed_Material for content matching the task topic
2. WHEN generating suggestions, THE Suggestion_Engine SHALL use the Student_Profile to filter content appropriate for the student's level
3. THE Suggestion_Engine SHALL rank suggestions based on teaching style and relevance to current knowledge gaps
4. THE Suggestion_Engine SHALL return suggestions with metadata including video title, channel, topics covered, and teaching approach
5. WHEN no relevant material is found, THE Suggestion_Engine SHALL return a message indicating no matches
6. WHEN semantic search is enabled, THE Suggestion_Engine SHALL use vector embeddings to find semantically similar content
7. WHEN using RAG, THE Suggestion_Engine SHALL retrieve relevant context and use it to enhance recommendation quality

### Requirement 4: Vector Database and Semantic Search

**User Story:** As a student, I want the system to understand the meaning of my learning needs, so that I receive more relevant content recommendations beyond keyword matching.

#### Acceptance Criteria

1. THE Vector_DB_Module SHALL provide a consistent interface for vector database operations
2. WHEN running locally, THE Vector_DB_Module SHALL use an in-memory vector store for development
3. WHEN running on Lambda, THE Vector_DB_Module SHALL use a cloud vector database (Pinecone or Weaviate)
4. WHEN indexing a video, THE Indexing_Worker SHALL generate vector embeddings from the video content
5. WHEN searching for materials, THE Suggestion_Engine SHALL support both keyword-based and semantic search
6. THE Vector_DB_Module SHALL support filtering by metadata (difficulty level, teaching style, topics)
7. WHEN generating embeddings, THE AI_Module SHALL use the configured embedding model

### Requirement 5: Progress Verification and Profile Updates

**User Story:** As a student, I want to verify my learning after studying materials, so that I can confirm I've acquired the necessary skills.

#### Acceptance Criteria

1. WHEN a student indicates readiness to progress, THE Quiz_Module SHALL generate a Progress_Quiz for the current task
2. WHEN a Progress_Quiz is completed, THE Quiz_Module SHALL evaluate if the student has acquired the required skills
3. WHEN a Progress_Quiz is passed, THE Quiz_Module SHALL update the Student_Profile marking the task as complete
4. WHEN a Progress_Quiz is passed, THE Quiz_Module SHALL advance the student to the next task in the Learning_Plan
5. IF a Progress_Quiz is failed, THEN THE Quiz_Module SHALL update the Student_Profile with identified skill gaps

### Requirement 6: Adaptive Re-teaching

**User Story:** As a student, I want alternative learning materials when I struggle, so that I can approach difficult concepts from different angles.

#### Acceptance Criteria

1. WHEN a Progress_Quiz is failed, THE Suggestion_Engine SHALL generate new content recommendations
2. WHEN generating retry recommendations, THE Suggestion_Engine SHALL exclude previously suggested materials
3. WHEN generating retry recommendations, THE Suggestion_Engine SHALL prioritize different teaching styles from previous suggestions
4. THE Suggestion_Engine SHALL use the failed quiz results to identify specific sub-topics needing reinforcement
5. WHEN a student fails multiple attempts, THE Suggestion_Engine SHALL suggest foundational prerequisite materials

### Requirement 7: YouTube Content Indexing

**User Story:** As a system administrator, I want to index YouTube educational content, so that the suggestion engine has a rich database of learning materials.

#### Acceptance Criteria

1. WHEN a YouTube channel URL is provided, THE Indexing_Worker SHALL extract metadata for all videos in the channel
2. WHEN processing a video, THE Indexing_Worker SHALL identify topics covered in the content
3. WHEN processing a video, THE Indexing_Worker SHALL classify the teaching style and approach
4. WHEN indexing is complete, THE Indexing_Worker SHALL persist extracted data to Indexed_Material storage
5. IF a video cannot be processed, THEN THE Indexing_Worker SHALL log the error and continue with remaining videos

### Requirement 8: Chat-Based User Interface

**User Story:** As a student, I want to interact with the tutor through a chat interface, so that I can access all system features conversationally.

#### Acceptance Criteria

1. WHEN a student sends a message, THE Frontend SHALL route the request to the appropriate system component
2. THE Frontend SHALL display quiz questions in a readable format with clear answer options
3. THE Frontend SHALL display content suggestions as a list with clickable video links
4. THE Frontend SHALL show the student's current task and progress within the Learning_Path
5. WHEN system processing occurs, THE Frontend SHALL provide feedback indicating activity

### Requirement 9: Storage Abstraction Layer

**User Story:** As a developer, I want a unified storage interface, so that the system works seamlessly in both local development and Lambda deployment.

#### Acceptance Criteria

1. THE Memory_Module SHALL provide a consistent interface for all storage operations
2. WHEN running locally, THE Memory_Module SHALL use the Local_Wrapper for file system storage
3. WHEN running on Lambda, THE Memory_Module SHALL use Cloud_Storage for persistent storage
4. THE Memory_Module SHALL support operations for storing, retrieving, updating, and deleting Student_Profiles, Learning_Paths, and Indexed_Material
5. WHEN a storage operation fails, THE Memory_Module SHALL return a descriptive error without exposing implementation details

### Requirement 10: AI Provider Abstraction Layer

**User Story:** As a developer, I want to support multiple AI providers, so that the system can use different AI services based on configuration.

#### Acceptance Criteria

1. THE AI_Module SHALL provide a consistent interface for AI operations across all providers
2. THE AI_Module SHALL support OpenRouter as an AI provider
3. THE AI_Module SHALL support Claude as an AI provider
4. WHEN configured, THE AI_Module SHALL select the appropriate provider based on configuration settings
5. WHEN an AI request fails, THE AI_Module SHALL return a descriptive error without exposing provider-specific details
6. THE AI_Module SHALL support streaming responses for real-time chat interaction

### Requirement 11: Modular Architecture

**User Story:** As a developer, I want a modular system architecture, so that components can be developed, tested, and deployed independently.

#### Acceptance Criteria

1. THE Memory_Module SHALL exist as a separate C# project with no dependencies on other modules
2. THE AI_Module SHALL exist as a separate C# project with no dependencies on other modules except Memory_Module
3. THE Quiz_Module SHALL reference the Memory_Module and AI_Module as dependencies
4. THE Suggestion_Engine SHALL reference the Memory_Module and AI_Module as dependencies
5. THE Indexing_Worker SHALL reference the Memory_Module and AI_Module as dependencies
6. WHEN a module interface changes, THEN dependent modules SHALL remain unaffected if the interface contract is maintained

### Requirement 12: Dual Execution Environment Support

**User Story:** As a developer, I want to run the system both locally and on AWS Lambda, so that I can develop efficiently and deploy to production.

#### Acceptance Criteria

1. WHEN running locally, THE Tutor_System SHALL execute without requiring AWS credentials or services
2. WHEN deployed to Lambda, THE Tutor_System SHALL utilize AWS services for storage and execution
3. THE Tutor_System SHALL use environment-based configuration to determine execution mode
4. WHEN switching between local and Lambda execution, THE Tutor_System SHALL require no code changes
5. THE Tutor_System SHALL provide logging appropriate to the execution environment

### Requirement 13: SOLID Principles Compliance

**User Story:** As a developer, I want the codebase to follow SOLID principles, so that the system is maintainable, testable, and extensible.

#### Acceptance Criteria

1. THE Memory_Module SHALL depend on abstractions rather than concrete implementations
2. THE AI_Module SHALL depend on abstractions rather than concrete implementations
3. WHEN adding a new storage provider, THE Memory_Module SHALL require no modifications to existing code
4. WHEN adding a new AI provider, THE AI_Module SHALL require no modifications to existing code
5. THE Tutor_System SHALL use dependency injection for component composition

### Requirement 14: Error Handling and Resilience

**User Story:** As a user, I want the system to handle errors gracefully, so that temporary failures don't disrupt my learning experience.

#### Acceptance Criteria

1. WHEN a component encounters an error, THE Tutor_System SHALL log the error with sufficient context for debugging
2. WHEN an AI provider is unavailable, THE Tutor_System SHALL return a user-friendly error message
3. WHEN storage operations fail, THE Tutor_System SHALL retry transient failures up to three times
4. IF all retries fail, THEN THE Tutor_System SHALL return an error without corrupting existing Student_Profile data
5. WHEN the Indexing_Worker encounters an invalid video, THE Indexing_Worker SHALL skip it and continue processing
