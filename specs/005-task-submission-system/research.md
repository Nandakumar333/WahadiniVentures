# Implementation Research: Task Submission System

**Feature**: `005-task-submission-system`
**Date**: 2025-11-28

## Technology Choices

### Backend Framework
- **Decision**: .NET 8 C#, ASP.NET Core Web API
- **Rationale**: Standard for the project.
- **Alternatives**: Node.js (rejected for consistency).

### Frontend Framework
- **Decision**: React 18, TypeScript
- **Rationale**: Standard for the project.

### Form Handling
- **Decision**: React Hook Form with Zod validation.
- **Rationale**: Best performance for complex/dynamic forms, good TypeScript integration.
- **Research**: Investigated dynamic field array patterns for handling different task types.

### Database Storage
- **Decision**: PostgreSQL with JSONB for `TaskData` and `SubmissionData`.
- **Rationale**: Allows flexibility for different task types (Quiz vs. Text) without creating multiple tables for each type.
- **Alternatives**: EAV pattern (too complex), NoSQL (project uses Postgres).

### State Management
- **Decision**: React Query (TanStack Query).
- **Rationale**: Efficient server state management, caching, and background updates for submission status.

### Concurrency Control
- **Decision**: Optimistic Locking (using a version/timestamp field).
- **Rationale**: Prevents admin review conflicts without blocking UI.

### File Storage
- **Decision**: Local filesystem for MVP (with interface for cloud).
- **Rationale**: Simple start; aligned with project's current capability.

## Unknowns & Resolutions

- **Q**: Best pattern for dynamic forms in React?
- **A**: Use a "Form Factory" component that switches on `taskType` and renders specific sub-forms (QuizForm, TextForm, etc.) utilizing a common `useFormContext`.

- **Q**: How to implement Optimistic Locking in EF Core?
- **A**: Use a `[Timestamp]` or concurrency token property on the `Submission` entity. Handle `DbUpdateConcurrencyException`.
