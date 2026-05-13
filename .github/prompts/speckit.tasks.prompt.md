---
description: Generate an actionable, dependency-ordered tasks.md for the feature based on available design artifacts.
---

## User Input

```text
$ARGUMENTS
```

You **MUST** consider the user input before proceeding (if not empty).

## Outline

1. **Setup**: Run `.specify/scripts/powershell/check-prerequisites.ps1 -Json` from repo root and parse FEATURE_DIR and AVAILABLE_DOCS list. All paths must be absolute. For single quotes in args like "I'm Groot", use escape syntax: e.g 'I'\''m Groot' (or double-quote if possible: "I'm Groot").

2. **Load design documents**: Read from FEATURE_DIR:
   - **Required**: `.github/prompts/architecture.prompt.md` (Clean Architecture patterns, code standards)
   - **Required**: `.github/prompts/backend.prompt.md` (Backend implementation patterns: entities, services, controllers, repositories)
   - **Required**: `.github/prompts/frontend.prompt.md` (Frontend implementation patterns: components, hooks, state management)
   - **Required**: `.github/prompts/database.prompt.md` (Database design, migrations, EF Core patterns)
   - **Required**: `.github/prompts/testing.prompt.md` (Testing strategies: unit, integration, E2E)
   - **Required**: `.github/prompts/security.prompt.md` (Security best practices: auth, validation, protection)
   - **Required**: plan.md (tech stack, libraries, structure), spec.md (user stories with priorities)
   - **Optional**: data-model.md (entities), contracts/ (API endpoints), research.md (decisions), quickstart.md (test scenarios)
   - **If Deployment**: `.github/prompts/deployment.prompt.md` (Build and deployment procedures)
   - **If CI/CD**: `.github/prompts/ci-cd.prompt.md` (Pipeline configuration)
   - Note: Not all projects have all documents. Generate tasks based on what's available.

3. **Execute task generation workflow**:
   - Load plan.md and extract tech stack, libraries, project structure
   - Load spec.md and extract user stories with their priorities (P1, P2, P3, etc.)
   - If data-model.md exists: Extract entities and map to user stories
   - If contracts/ exists: Map endpoints to user stories
   - If research.md exists: Extract decisions for setup tasks
   - Generate tasks organized by user story (see Task Generation Rules below)
   - Generate dependency graph showing user story completion order
   - Create parallel execution examples per user story
   - Validate task completeness (each user story has all needed tasks, independently testable)

4. **Generate tasks.md**: Use `.specify.specify/templates/tasks-template.md` as structure, fill with:
   - Correct feature name from plan.md
   - Phase 1: Setup tasks (project initialization)
   - Phase 2: Foundational tasks (blocking prerequisites for all user stories)
   - Phase 3+: One phase per user story (in priority order from spec.md)
   - Each phase includes: story goal, independent test criteria, tests (if requested), implementation tasks
   - Final Phase: Polish & cross-cutting concerns
   - All tasks must follow the strict checklist format (see Task Generation Rules below)
   - Clear file paths for each task
   - Dependencies section showing story completion order
   - Parallel execution examples per story
   - Implementation strategy section (MVP first, incremental delivery)

5. **Report**: Output path to generated tasks.md and summary:
   - Total task count
   - Task count per user story
   - Parallel opportunities identified
   - Independent test criteria for each story
   - Suggested MVP scope (typically just User Story 1)
   - Format validation: Confirm ALL tasks follow the checklist format (checkbox, ID, labels, file paths)

Context for task generation: $ARGUMENTS

The tasks.md should be immediately executable - each task must be specific enough that an LLM can complete it without additional context.

## Task Generation Rules

**CRITICAL**: Tasks MUST be organized by user story to enable independent implementation and testing.

**Tests are OPTIONAL**: Only generate test tasks if explicitly requested in the feature specification or if user requests TDD approach.

### Checklist Format (REQUIRED)

Every task MUST strictly follow this format:

```text
- [ ] [TaskID] [P?] [Story?] Description with file path
```

**Format Components**:

1. **Checkbox**: ALWAYS start with `- [ ]` (markdown checkbox)
2. **Task ID**: Sequential number (T001, T002, T003...) in execution order
3. **[P] marker**: Include ONLY if task is parallelizable (different files, no dependencies on incomplete tasks)
4. **[Story] label**: REQUIRED for user story phase tasks only
   - Format: [US1], [US2], [US3], etc. (maps to user stories from spec.md)
   - Setup phase: NO story label
   - Foundational phase: NO story label  
   - User Story phases: MUST have story label
   - Polish phase: NO story label
5. **Description**: Clear action with exact file path

**Examples**:

- ✅ CORRECT: `- [ ] T001 Create project structure per implementation plan`
- ✅ CORRECT: `- [ ] T005 [P] Implement authentication middleware in src/middleware/auth.py`
- ✅ CORRECT: `- [ ] T012 [P] [US1] Create User model in src/models/user.py`
- ✅ CORRECT: `- [ ] T014 [US1] Implement UserService in src/services/user_service.py`
- ❌ WRONG: `- [ ] Create User model` (missing ID and Story label)
- ❌ WRONG: `T001 [US1] Create model` (missing checkbox)
- ❌ WRONG: `- [ ] [US1] Create User model` (missing Task ID)
- ❌ WRONG: `- [ ] T001 [US1] Create model` (missing file path)

### Task Organization

1. **From User Stories (spec.md)** - PRIMARY ORGANIZATION:
   - Each user story (P1, P2, P3...) gets its own phase
   - Map all related components to their story following Clean Architecture layers:
     - Domain entities (models) with factory methods and business logic
     - Repository interfaces in Domain layer
     - Application services (business orchestration, CQRS commands/queries)
     - Infrastructure repositories (EF Core implementations)
     - API Controllers (presentation layer) with proper authorization
     - Frontend components (feature-based modules) if applicable
     - If tests requested: Unit tests (domain/service), Integration tests (repository/API), Component tests (UI)
   - Mark story dependencies (most stories should be independent)

2. **From Contracts**:
   - Map each contract/endpoint → to the user story it serves
   - If tests requested: Each contract → contract test task [P] before implementation in that story's phase

3. **From Data Model**:
   - Map each entity to the user story(ies) that need it
   - If entity serves multiple stories: Put in earliest story or Setup phase
   - Relationships → service layer tasks in appropriate story phase

4. **From Setup/Infrastructure**:
   - Shared infrastructure → Setup phase (Phase 1)
   - Foundational/blocking tasks → Foundational phase (Phase 2)
   - Story-specific setup → within that story's phase

### Phase Structure

- **Phase 1**: Setup (project initialization following WahadiniCryptoQuest structure)
  - Backend: Create solution with 4 projects (API, Application, Domain, Infrastructure)
  - Frontend: Initialize React+Vite+TypeScript with feature-based structure
  - Database: Setup PostgreSQL connection and initial context
  - Dependencies: Install NuGet packages (.NET) and npm packages (React)
  
- **Phase 2**: Foundational (blocking prerequisites - MUST complete before user stories)
  - Base entities and common interfaces (IRepository, IUnitOfWork)
  - Authentication/Authorization infrastructure (JWT, ASP.NET Identity)
  - Database context and configurations
  - API base controllers and middleware
  - Frontend routing and authentication guards
  
- **Phase 3+**: User Stories in priority order (P1, P2, P3...)
  - Within each story (following Clean Architecture layers):
    1. Tests (if requested): Unit tests → Integration tests → Component tests
    2. Domain: Entities with business logic, Value objects, Domain events
    3. Application: Service interfaces, DTOs, Validators (FluentValidation)
    4. Infrastructure: Repository implementations, EF Core configurations
    5. Presentation: API Controllers with authorization, Request/Response models
    6. Frontend: Feature components, hooks, services (if applicable)
  - Each phase should be a complete, independently testable increment
  
- **Final Phase**: Polish & Cross-Cutting Concerns
  - Performance optimization (caching, query optimization)
  - Security hardening (input validation, SQL injection prevention)
  - Documentation (XML docs, API documentation)
  - Error handling improvements
  - Logging enhancements
