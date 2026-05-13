# Tasks: Task Submission System

**Branch**: `005-task-submission-system`
**Spec**: [Link](../spec.md)
**Plan**: [Link](../plan.md)

## Phase 1: Setup & Configuration (2 hours)

**Goal**: Initialize project structure and install necessary dependencies.

- [x] T001 Install backend dependencies (FluentValidation) in `backend/src/WahadiniCryptoQuest.Service/WahadiniCryptoQuest.Service.csproj`
- [x] T002 Install frontend dependencies (React Hook Form, Zod) in `frontend/package.json`
- [x] T003 Create feature directory structure in backend and frontend

## Phase 2: Foundational Tasks (10 hours)

**Goal**: Implement core entities, repositories, and services required by all user stories.
**Independent Test**: Database migrations apply successfully, and base services can be instantiated.

- [x] T004 Update `Task` entity with `TaskType`, `TaskData` (JSONB), and `PassingScoreThreshold` in `backend/src/WahadiniCryptoQuest.Core/Entities/Task.cs`
- [x] T005 Create `UserTaskSubmission` entity with `SubmissionData` (JSONB), `Status`, and `Version` (concurrency token) in `backend/src/WahadiniCryptoQuest.Core/Entities/UserTaskSubmission.cs`
- [x] T006 Create `PointTransaction` entity in `backend/src/WahadiniCryptoQuest.Core/Entities/PointTransaction.cs`
- [x] T007 Create and apply EF Core migration for new entities in `backend/src/WahadiniCryptoQuest.DAL/Migrations/`
- [x] T008 Create unit tests for new Repository methods in `backend/tests/WahadiniCryptoQuest.DAL.Tests/TaskRepositoryTests.cs`
- [x] T009 Create `ITaskRepository` and `ITaskSubmissionRepository` interfaces in `backend/src/WahadiniCryptoQuest.Core/Interfaces/`
- [x] T010 Implement `TaskRepository` in `backend/src/WahadiniCryptoQuest.DAL/Repositories/TaskRepository.cs`
- [x] T011 Implement `TaskSubmissionRepository` in `backend/src/WahadiniCryptoQuest.DAL/Repositories/TaskSubmissionRepository.cs`
- [x] T012 Create `IFileStorageService` interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/IFileStorageService.cs`
- [x] T013 Implement `FileStorageService` (Local) in `backend/src/WahadiniCryptoQuest.Service/Services/FileStorageService.cs`
- [x] T014 Create `ITaskSubmissionService` interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/ITaskSubmissionService.cs`
- [x] T015 Implement `TaskSubmissionService` skeleton in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`

## Phase 3: User Story 1 - Submit Basic Tasks (10 hours)

**Goal**: Enable users to submit Quiz, Text, Link, and Wallet tasks.
**Independent Test**: User can submit each task type via API/UI and see correct status (Approved/Pending).

- [x] T016 [US1] Create unit tests for Quiz auto-grading logic in `backend/tests/WahadiniCryptoQuest.Service.Tests/QuizGradingTests.cs`
- [x] T017 [US1] Create unit tests for validation logic (Text/Link/Wallet) in `backend/tests/WahadiniCryptoQuest.Service.Tests/SubmissionValidationTests.cs`
- [x] T018 [US1] Create DTOs (`QuizSubmissionDto`, `TextSubmissionDto`, etc.) in `backend/src/WahadiniCryptoQuest.Core/DTOs/`
- [x] T019 [US1] Implement `SubmitQuizAsync` with auto-grading logic in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T020 [US1] Implement `SubmitTextAsync` and `SubmitExternalLinkAsync` in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T021 [US1] Implement `SubmitWalletAsync` in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T022 [US1] Create `TaskSubmissionsController` with `Submit` endpoint in `backend/src/WahadiniCryptoQuest.API/Controllers/TaskSubmissionsController.cs`
- [x] T023 [US1] Create `QuizTaskForm` component in `frontend/src/components/tasks/QuizTaskForm.tsx`
- [x] T024 [US1] Create `TextTaskForm` component in `frontend/src/components/tasks/TextTaskForm.tsx`
- [x] T025 [US1] Create `ExternalLinkTaskForm` component in `frontend/src/components/tasks/ExternalLinkTaskForm.tsx`
- [x] T026 [US1] Create `WalletTaskForm` component in `frontend/src/components/tasks/WalletTaskForm.tsx`
- [x] T027 [US1] Create `TaskSubmissionModal` wrapper component in `frontend/src/components/tasks/TaskSubmissionModal.tsx`
- [x] T028 [US1] Integrate submission API calls in frontend services `frontend/src/services/api/submissionService.ts`

## Phase 4: User Story 2 - Admin Review & Approval (8 hours)

**Goal**: Enable admins to review, approve, and reject manual submissions.
**Independent Test**: Admin can change submission status and user receives points upon approval.

- [x] T029 [US2] Create unit tests for `ReviewSubmissionAsync` (status change & points) in `backend/tests/WahadiniCryptoQuest.Service.Tests/AdminReviewTests.cs`
- [x] T030 [US2] Implement `GetPendingSubmissionsAsync` in `backend/src/WahadiniCryptoQuest.DAL/Repositories/TaskSubmissionRepository.cs`
- [x] T031 [US2] Implement `ReviewSubmissionAsync` with optimistic locking in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T032 [US2] Integrate `PointTransaction` creation on approval in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T033 [US2] Create admin endpoints (`GetPending`, `Review`) in `backend/src/WahadiniCryptoQuest.API/Controllers/AdminTaskSubmissionsController.cs`
- [x] T034 [US2] Create `AdminTasksPage` layout in `frontend/src/pages/admin/AdminTasksPage.tsx`
- [x] T035 [US2] Create `SubmissionsTable` component with filtering in `frontend/src/components/admin/SubmissionsTable.tsx`
- [x] T036 [US2] Create `SubmissionPreviewModal` for review actions in `frontend/src/components/admin/SubmissionPreviewModal.tsx`

## Phase 5: User Story 3 - Submit Screenshot Evidence (6 hours)

**Goal**: Enable users to upload image files for verification.
**Independent Test**: User can upload a valid image file; invalid files are rejected.

- [x] T037 [US3] Create integration tests for file upload controller actions in `backend/tests/WahadiniCryptoQuest.API.Tests/Controllers/FileUploadTests.cs`
- [x] T038 [US3] Implement `SubmitScreenshotAsync` with file validation in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T039 [US3] Update `TaskSubmissionsController` to handle multipart/form-data in `backend/src/WahadiniCryptoQuest.API/Controllers/TaskSubmissionsController.cs`
- [x] T040 [US3] Create `ScreenshotTaskForm` with file input in `frontend/src/components/tasks/ScreenshotTaskForm.tsx`
- [x] T041 [US3] Implement client-side retry logic for uploads in `frontend/src/services/api/submissionService.ts`

## Phase 6: User Story 4 - Resubmission Workflow (4 hours)

**Goal**: Allow users to fix rejected submissions while blocking duplicates.
**Independent Test**: Rejected submission shows "Resubmit" button; Pending/Approved submissions block new attempts.

- [x] T042 [US4] Create unit tests for `BlockActiveSubmission` logic in `backend/tests/WahadiniCryptoQuest.Service.Tests/ResubmissionTests.cs`
- [x] T043 [US4] Implement `BlockActiveSubmission` check in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T044 [US4] Update `MySubmissions` endpoint to include feedback in `backend/src/WahadiniCryptoQuest.API/Controllers/TaskSubmissionsController.cs`
- [x] T045 [US4] Create `MySubmissionsPage` to list user history in `frontend/src/pages/MySubmissionsPage.tsx`
- [x] T046 [US4] Add "Resubmit" logic to `TaskCard` component in `frontend/src/components/tasks/TaskCard.tsx`

## Phase 7: User Story 5 - Admin Bulk Actions & Filtering (4 hours)

**Goal**: Improve admin efficiency with bulk operations.
**Independent Test**: Admin can select multiple submissions and approve/reject them in one action.

- [x] T047 [US5] Create unit tests for `BulkReviewAsync` in `backend/tests/WahadiniCryptoQuest.Service.Tests/Services/BulkActionTests.cs`
- [x] T048 [US5] Implement `BulkReviewAsync` in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`
- [x] T049 [US5] Add bulk review endpoint to `backend/src/WahadiniCryptoQuest.API/Controllers/AdminTaskSubmissionsController.cs`
- [x] T050 [US5] Add bulk selection checkboxes to `SubmissionsTable` in `frontend/src/components/admin/SubmissionsTable.tsx`
- [x] T051 [US5] Implement bulk action toolbar in `frontend/src/pages/admin/AdminTasksPage.tsx`

## Phase 8: Polish & Cross-Cutting Concerns (4 hours)

**Goal**: Finalize UI/UX, ensure performance, and verify all requirements.

- [x] T052 Implement background job for cleaning up rejected file payloads (FR-017) in `backend/src/WahadiniCryptoQuest.Worker/Jobs/FileCleanupJob.cs`
- [x] T053 Add loading states to all forms and tables
- [x] T054 Implement toast notifications for submission success/error
- [x] T055 Verify optimistic locking behavior (simulate concurrent edits)
- [x] T056 Clean up unused code and comments

## Dependencies

- **Phase 1 & 2** (Setup/Foundational) -> **Phase 3** (US1: Basic Submission)
- **Phase 3** (US1: Basic Submission) -> **Phase 4** (US2: Admin Review)
- **Phase 2** (Foundational) -> **Phase 5** (US3: File Upload) [Parallelizable with Phase 3/4]
- **Phase 3 & 4** -> **Phase 6** (US4: Resubmission)
- **Phase 4** -> **Phase 7** (US5: Bulk Actions)

## Implementation Strategy

- **MVP Scope**: Phases 1, 2, 3, and 4 (Basic submission and manual review).
- **Incremental Delivery**:
    1. Database & Core Services
    2. Basic Task Submission (Quiz/Text)
    3. Admin Review
    4. File Uploads
    5. Polish & Bulk Actions