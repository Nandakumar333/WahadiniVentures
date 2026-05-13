# Implementation Plan: Task Submission System - Lesson Integration

**Branch**: `005-task-submission-system` | **Date**: 2025-12-01 | **Spec**: [spec.md](spec.md)
**Input**: Feature specification from `/specs/005-task-submission-system/spec.md`

**Note**: This plan focuses on integrating the existing task submission system into lesson pages so users can discover and complete tasks while learning.

## Summary

The task submission system backend is fully implemented with support for 5 task types (Quiz, ExternalLink, Screenshot, TextSubmission, WalletVerification), but tasks are not visible or accessible from lesson pages. This integration will display tasks within lessons, allowing users to complete tasks immediately after watching video content, following the learn-then-practice workflow outlined in the spec.

## Technical Context

**Language/Version**: Backend: .NET 8 C# | Frontend: TypeScript 4.9+ with React 18  
**Primary Dependencies**: Entity Framework Core 8.0, AutoMapper, React Query 5, react-player, Zod validation  
**Storage**: PostgreSQL 15+ with JSONB for task data, existing LearningTask and UserTaskSubmission tables  
**Testing**: Backend: xUnit with FluentAssertions | Frontend: Vitest with React Testing Library  
**Target Platform**: Web application (desktop and mobile responsive)  
**Project Type**: Web (backend + frontend)  
**Performance Goals**: <200ms lesson load time with tasks, <100ms submission status check, support 1000+ concurrent users  
**Constraints**: Backward compatible with existing lesson API, no breaking changes to current task submission flow  
**Scale/Scope**: ~10 tasks per lesson max, support for 10,000+ enrolled users, <5MB file uploads for screenshots

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

✅ **Clean Architecture**: Maintains separation between Domain (LearningTask entity), Application (LessonService), Infrastructure (LessonRepository), and Presentation (LessonsController) layers

✅ **No Breaking Changes**: Extends existing endpoints with optional parameters, maintains backward compatibility

✅ **Security by Design**: Task access validated through lesson authorization, follows existing JWT authentication patterns

✅ **Test Coverage Required**: Unit tests for service layer, integration tests for API endpoints, component tests for React UI

✅ **Performance Standards**: Eager loading with EF Core to prevent N+1 queries, React Query caching to minimize API calls

✅ **Input Validation**: FluentValidation on backend, Zod schemas on frontend for all task submission data

**Gate Status**: PASS - No constitutional violations

## Project Structure

### Documentation (this feature)

```text
specs/005-task-submission-system/
├── plan.md              # This file (updated for lesson integration)
├── research.md          # Existing research on task system
├── data-model.md        # Phase 1 output (NEW - task integration data model)
├── quickstart.md        # Phase 1 output (NEW - integration setup guide)
├── contracts/           # Phase 1 output (NEW - API contract updates)
│   ├── lessons-api.yaml # Updated lesson endpoints
│   └── tasks-api.yaml   # Task submission status endpoint
└── tasks.md             # Phase 2 output (created by /speckit.tasks command)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── WahadiniCryptoQuest.API/
│   │   └── Controllers/
│   │       ├── LessonsController.cs          # UPDATE: Add includeTasks param
│   │       └── TaskSubmissionsController.cs # UPDATE: Add status endpoint
│   │
│   ├── WahadiniCryptoQuest.Core/
│   │   ├── DTOs/
│   │   │   ├── Course/
│   │   │   │   └── LessonDto.cs             # UPDATE: Add Tasks property
│   │   │   └── Task/
│   │   │       ├── LearningTaskDto.cs       # NEW
│   │   │       └── TaskSubmissionStatusDto.cs # NEW
│   │   └── Interfaces/
│   │       └── Services/
│   │           ├── ILessonService.cs        # UPDATE: Add includeTasks param
│   │           └── ITaskSubmissionService.cs # UPDATE: Add GetStatusAsync
│   │
│   ├── WahadiniCryptoQuest.Service/
│   │   ├── Lesson/
│   │   │   └── LessonService.cs             # UPDATE: Extend GetByIdAsync
│   │   ├── Services/
│   │   │   └── TaskSubmissionService.cs     # UPDATE: Add status check
│   │   └── Mappings/
│   │       └── TaskMappingProfile.cs        # UPDATE: Add Task → DTO mapping
│   │
│   └── WahadiniCryptoQuest.DAL/
│       └── Repositories/
│           └── LessonRepository.cs          # EXISTING: GetWithTasksAsync
│
└── tests/
    ├── WahadiniCryptoQuest.API.Tests/
    │   └── Controllers/
    │       └── LessonsControllerTests.cs    # UPDATE: Test includeTasks
    ├── WahadiniCryptoQuest.Service.Tests/
    │   └── Lesson/
    │       └── LessonServiceTests.cs        # UPDATE: Test with tasks
    └── WahadiniCryptoQuest.DAL.Tests/
        └── Repositories/
            └── LessonRepositoryTests.cs     # EXISTING: Tests exist

frontend/
├── src/
│   ├── components/
│   │   ├── lesson/
│   │   │   ├── LessonPlayer/
│   │   │   │   └── LessonPlayer.tsx         # EXISTING: Video player
│   │   │   └── LessonTasks/
│   │   │       ├── LessonTasksSection.tsx   # NEW
│   │   │       ├── LessonTasksList.tsx      # NEW
│   │   │       └── index.ts                 # NEW
│   │   └── tasks/
│   │       ├── TaskCard.tsx                 # EXISTING: Reuse
│   │       └── TaskSubmissionModal.tsx      # EXISTING: Reuse
│   │
│   ├── hooks/
│   │   └── lesson/
│   │       ├── useLesson.ts                 # UPDATE: Add includeTasks option
│   │       ├── useLessonTasks.ts            # NEW
│   │       └── useTaskSubmissionStatus.ts   # NEW
│   │
│   ├── services/
│   │   └── api/
│   │       ├── lessonService.ts             # UPDATE: Add includeTasks param
│   │       └── taskService.ts               # UPDATE: Add getStatus method
│   │
│   ├── types/
│   │   ├── lesson.ts                        # UPDATE: Add tasks property
│   │   └── task.ts                          # UPDATE: Add status type
│   │
│   └── pages/
│       └── lesson/
│           └── LessonPage.tsx               # UPDATE: Display tasks section
│
└── tests/
    ├── components/
    │   └── lesson/
    │       └── LessonTasksSection.test.tsx  # NEW
    └── hooks/
        └── useLessonTasks.test.ts           # NEW
```

**Structure Decision**: Web application structure with backend (.NET 8 Web API) and frontend (React 18 TypeScript) following clean architecture principles. Updates focus on extending existing lesson display to include task integration while reusing existing task submission components.

## Complexity Tracking

**No violations** - This integration extends existing architecture without adding complexity:

- Reuses existing components (TaskCard, TaskSubmissionModal)
- Follows established patterns (DTOs, AutoMapper, React Query)
- No new dependencies required
- Maintains backward compatibility
- No database schema changes needed

## Phase 0: Research (Completed)

✅ Research file already exists at `research.md` documenting:
- Task submission system patterns (Quiz, Screenshot, Text, Link, Wallet)
- Form handling with React Hook Form and Zod
- Database storage using JSONB in PostgreSQL
- State management with React Query
- Optimistic locking for admin reviews

**Additional research for lesson integration:**
- Eager loading strategy to prevent N+1 queries
- Optional parameter pattern for backward compatibility
- React Query caching strategy (5-10 minute stale time)
- Task access validation through lesson authorization
- UI placement: accordion section below video player

## Phase 1: Design & Contracts (Completed)

### Data Model
✅ **data-model.md** already exists with core entities (LearningTask, UserTaskSubmission, PointTransaction)

**New DTOs documented in contracts:**
- `LearningTaskDto` - Task information transfer object
- `TaskSubmissionStatusDto` - User submission status
- Updated `LessonDto` - Added optional Tasks property

### API Contracts
✅ Created OpenAPI specifications:

**contracts/lessons-api.yaml**
- Updated `GET /api/lessons/{id}` endpoint
- Added `includeTasks` query parameter (boolean, default: false)
- Documented LessonDto with optional Tasks array
- Includes examples with and without tasks

**contracts/tasks-status-api.yaml**
- New `GET /api/tasks/{taskId}/submission-status` endpoint
- Returns TaskSubmissionStatusDto
- Handles null status (never submitted) scenario
- Documents all submission states (Pending, Approved, Rejected)

### Quickstart Guide
✅ **quickstart.md** exists with task submission system setup

**Additional integration steps added:**
1. Backend: Update LessonDto, LessonService, LessonsController
2. Create TaskSubmissionStatusDto and new endpoint
3. Frontend: Update types, services, hooks
4. Create LessonTasksSection component
5. Update LessonPage to display tasks

### Agent Context
✅ Updated `.github/agents/copilot-instructions.md` with:
- React Query 5 for server state management
- Entity Framework Core 8.0 patterns
- PostgreSQL JSONB usage for task data
- Integration notes for this feature

## Implementation Phases

### Phase 2: Backend Implementation
**Files to modify/create:**
- [ ] Core/DTOs/Task/LearningTaskDto.cs (NEW)
- [ ] Core/DTOs/Task/TaskSubmissionStatusDto.cs (NEW)
- [ ] Core/DTOs/Course/LessonDto.cs (UPDATE - add Tasks property)
- [ ] Core/Interfaces/Services/ILessonService.cs (UPDATE - add includeTasks param)
- [ ] Service/Lesson/LessonService.cs (UPDATE - implement includeTasks logic)
- [ ] Service/Mappings/TaskMappingProfile.cs (UPDATE - add Task mappings)
- [ ] API/Controllers/LessonsController.cs (UPDATE - add query parameter)
- [ ] API/Controllers/TaskSubmissionsController.cs (UPDATE - add status endpoint)

**Tests to create:**
- [ ] Service.Tests/Lesson/LessonServiceTests.cs (UPDATE - test includeTasks)
- [ ] API.Tests/Controllers/LessonsControllerTests.cs (UPDATE - test query param)
- [ ] API.Tests/Controllers/TaskSubmissionsControllerTests.cs (UPDATE - test status)

### Phase 3: Frontend Implementation
**Files to modify/create:**
- [ ] types/lesson.ts (UPDATE - add tasks property)
- [ ] types/task.ts (UPDATE - add status types)
- [ ] services/api/lessonService.ts (UPDATE - add includeTasks param)
- [ ] services/api/taskService.ts (UPDATE - add getSubmissionStatus)
- [ ] hooks/lesson/useLessonTasks.ts (NEW)
- [ ] hooks/lesson/useTaskSubmissionStatus.ts (NEW)
- [ ] components/lesson/LessonTasks/LessonTasksSection.tsx (NEW)
- [ ] components/lesson/LessonTasks/index.ts (NEW)
- [ ] pages/lesson/LessonPage.tsx (UPDATE - add tasks section)
- [ ] components/layout/Sidebar.tsx (DONE - fixed Tasks link)

**Tests to create:**
- [ ] __tests__/components/lesson/LessonTasksSection.test.tsx (NEW)
- [ ] __tests__/hooks/useLessonTasks.test.ts (NEW)

## Success Metrics

**Performance:**
- Lesson load time with tasks: <200ms (P95)
- Task submission status check: <100ms (P95)
- Support 1000+ concurrent users

**User Experience:**
- Tasks visible within lesson page
- Submission status updates without full page reload
- Reuse existing task submission modal (consistent UX)

**Code Quality:**
- Maintain >80% test coverage
- No breaking changes to existing API
- Follow clean architecture principles

## Branch & Next Steps

**Branch**: `005-task-submission-system`  
**Current Status**: Phase 1 Complete (Plan, Contracts, Design)

**Next Command**: `/speckit.tasks` to generate implementation tasks

**Dependencies:**
- No external dependencies
- No database migrations required
- All infrastructure exists

**Estimated Effort:**
- Backend: 4-6 hours (DTOs, service updates, controller changes, tests)
- Frontend: 6-8 hours (types, hooks, components, integration, tests)
- Total: 10-14 hours for complete integration

**Rollout Plan:**
1. Backend implementation and testing
2. Frontend implementation and testing
3. Integration testing (E2E)
4. Deploy to staging
5. User acceptance testing
6. Production deployment

| Violation | Why Needed | Simpler Alternative Rejected Because |
|-----------|------------|-------------------------------------|
| [e.g., 4th project] | [current need] | [why 3 projects insufficient] |
| [e.g., Repository pattern] | [specific problem] | [why direct DB access insufficient] |
