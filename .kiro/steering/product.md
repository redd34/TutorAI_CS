# Product Overview

The AI Tutor System is a serverless C# application that provides structured, personalized learning through predefined curricula with AI-powered content recommendations and assessments. It should work for bilingual students and learn the study patterns for better outcomes.
The goal is to create a personal tutor to help fill in knowledge gaps.

## Core Functionality

- Students select a learning path (e.g., Python) and complete a baseline assessment
- System creates personalized learning plans based on identified knowledge gaps
- AI-powered suggestion engine recommends relevant educational content (YouTube videos)
- Progress quizzes verify skill acquisition after each learning task
- Adaptive re-teaching provides alternative materials when students struggle
- Chat-based interface for conversational interaction

## Learning Flow

1. Select learning path → 2. Baseline assessment → 3. Personalized plan → 4. Content recommendations → 5. Study materials → 6. Progress quiz → 7. Update profile and advance (or retry with alternative materials)

## Deployment Modes

- **Local Development**: Runs on local machine with file system storage
- **AWS Lambda**: Serverless production deployment with S3/DynamoDB storage

## Key Design Principles

- Modular architecture with clear separation of concerns
- Environment-agnostic code that works seamlessly in both local and cloud environments
- SOLID principles for maintainability and extensibility
- Abstraction layers for storage, AI providers, and vector databases
