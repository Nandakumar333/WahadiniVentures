# Tasks: Task Submission System - Lesson Integration

**Input**: Design documents from `/specs/005-task-submission-system/`
**Prerequisites**: plan.md, spec.md, data-model.md, contracts/, quickstart.md

**Note**: The core task submission system is already implemented (see tasks-original.md). These tasks focus on integrating tasks into lesson pages so users can discover and complete tasks while learning.

## Format: `- [ ] [ID] [P?] Description`

- **[P]**: Can run in parallel (different files, no dependencies)
- Include exact file paths in descriptions

---

## Phase 1: Backend - DTOs and Mapping

**Purpose**: Create data transfer objects for task integration

- [X] T001 [P] Create LearningTaskDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Task/LearningTaskDto.cs`
- [X] T002 [P] Create TaskSubmissionStatusDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Task/TaskSubmissionStatusDto.cs`
- [X] T003 Update LessonDto to add Tasks property in `backend/src/WahadiniCryptoQuest.Core/DTOs/Course/LessonDto.cs`
- [X] T004 [P] Update TaskMappingProfile to map LearningTask to LearningTaskDto in `backend/src/WahadiniCryptoQuest.Service/Mappings/TaskMappingProfile.cs`
- [X] T005 [P] Update TaskMappingProfile to map UserTaskSubmission to TaskSubmissionStatusDto in `backend/src/WahadiniCryptoQuest.Service/Mappings/TaskMappingProfile.cs`
- [X] T006 Update LessonMappingProfile to include Tasks mapping in `backend/src/WahadiniCryptoQuest.Service/Mappings/LessonMappingProfile.cs`

---

## Phase 2: Backend - Service Layer

**Purpose**: Extend lesson and task services to support integration

- [X] T007 Update ILessonService interface to add includeTasks parameter in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/ILessonService.cs`
- [X] T008 Update LessonService.GetByIdAsync to support includeTasks parameter in `backend/src/WahadiniCryptoQuest.Service/Lesson/LessonService.cs`
- [X] T009 Add GetUserSubmissionForTaskAsync method to IUserTaskSubmissionRepository in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/IUserTaskSubmissionRepository.cs`
- [X] T010 Implement GetUserSubmissionForTaskAsync in UserTaskSubmissionRepository in `backend/src/WahadiniCryptoQuest.DAL/Repositories/UserTaskSubmissionRepository.cs`
- [X] T011 Add GetSubmissionStatusAsync method to ITaskSubmissionService in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Services/ITaskSubmissionService.cs`
- [X] T012 Implement GetSubmissionStatusAsync in TaskSubmissionService in `backend/src/WahadiniCryptoQuest.Service/Services/TaskSubmissionService.cs`

---

## Phase 3: Backend - API Controllers

**Purpose**: Expose new endpoints for lesson tasks and submission status

- [X] T013 Update LessonsController.GetById to add includeTasks query parameter in `backend/src/WahadiniCryptoQuest.API/Controllers/LessonsController.cs`
- [X] T014 Add GetSubmissionStatus endpoint to TaskSubmissionsController in `backend/src/WahadiniCryptoQuest.API/Controllers/TaskSubmissionsController.cs`

---

## Phase 4: Backend - Tests

**Purpose**: Ensure backend changes work correctly

- [X] T015 [P] Add test for LessonService.GetByIdAsync with includeTasks=true in `backend/tests/WahadiniCryptoQuest.Service.Tests/Lesson/LessonServiceTests.cs`
- [X] T016 [P] Add test for LessonService.GetByIdAsync with includeTasks=false in `backend/tests/WahadiniCryptoQuest.Service.Tests/Lesson/LessonServiceTests.cs`
- [X] T017 [P] Add test for LessonsController.GetById with includeTasks query param in `backend/tests/WahadiniCryptoQuest.API.Tests/Controllers/LessonsControllerTests.cs`
- [X] T018 [P] Add test for TaskSubmissionsController.GetSubmissionStatus endpoint in `backend/tests/WahadiniCryptoQuest.API.Tests/Controllers/TaskSubmissionsControllerTests.cs`
- [X] T019 [P] Add test for TaskSubmissionService.GetSubmissionStatusAsync in `backend/tests/WahadiniCryptoQuest.Service.Tests/Services/TaskSubmissionServiceTests.cs`

---

## Phase 5: Frontend - Type Definitions

**Purpose**: Update TypeScript types for lesson tasks

- [X] T020 [P] Add LearningTask interface to `frontend/src/types/task.ts`
- [X] T021 [P] Add TaskSubmissionStatus interface to `frontend/src/types/task.ts`
- [X] T022 Update Lesson interface to include tasks property in `frontend/src/types/lesson.ts`

---

## Phase 6: Frontend - API Services

**Purpose**: Update API services to support task integration

- [X] T023 Update lessonService.getById to add includeTasks parameter in `frontend/src/services/api/lessonService.ts`
- [X] T024 Add taskService.getSubmissionStatus method in `frontend/src/services/api/taskService.ts`

---

## Phase 7: Frontend - Custom Hooks

**Purpose**: Create React Query hooks for task data management

- [X] T025 [P] Create useLessonTasks hook in `frontend/src/hooks/lesson/useLessonTasks.ts`
- [X] T026 [P] Create useTaskSubmissionStatus hook in `frontend/src/hooks/lesson/useTaskSubmissionStatus.ts`
- [X] T027 Update useLesson hook to support includeTasks option in `frontend/src/hooks/lessons/useLesson.ts`

---

## Phase 8: Frontend - Task Components

**Purpose**: Create UI components for displaying tasks in lessons

- [X] T028 Create LessonTasksSection component in `frontend/src/components/lesson/LessonTasks/LessonTasksSection.tsx`
- [X] T029 Create TaskCardWithStatus wrapper component in `frontend/src/components/lesson/LessonTasks/LessonTasksSection.tsx`
- [X] T030 Create index.ts barrel export in `frontend/src/components/lesson/LessonTasks/index.ts`

---

## Phase 9: Frontend - Lesson Page Integration

**Purpose**: Integrate tasks into the lesson viewing experience

- [X] T031 Update LessonPage to load lesson with tasks in `frontend/src/pages/lesson/LessonPage.tsx`
- [X] T032 Add LessonTasksSection to LessonPage below video player in `frontend/src/pages/lesson/LessonPage.tsx`
- [X] T033 Add task submission modal state management to LessonPage in `frontend/src/pages/lesson/LessonPage.tsx`

---

## Phase 10: Frontend - Sidebar Fix

**Purpose**: Fix sidebar navigation to point to correct tasks page

- [X] T034 Update Sidebar Tasks link to point to /my-submissions in `frontend/src/components/layout/Sidebar.tsx`

---

## Phase 11: Frontend - Tests

**Purpose**: Ensure frontend components work correctly

- [X] T035 [P] Create test for LessonTasksSection component in `frontend/tests/components/lesson/LessonTasksSection.test.tsx`
- [X] T036 [P] Create test for useLessonTasks hook in `frontend/tests/hooks/useLessonTasks.test.ts`
- [X] T037 [P] Create test for useTaskSubmissionStatus hook in `frontend/tests/hooks/useTaskSubmissionStatus.test.ts`
- [X] T038 [P] Update LessonPage tests to verify tasks section rendering in `frontend/tests/pages/LessonPage.test.tsx`

---

## Phase 12: Integration & Polish

**Purpose**: End-to-end validation and cleanup

- [ ] T039 Test complete flow: Load lesson → View tasks → Submit task → Check status
- [ ] T040 Verify React Query caching works (5-10 minute stale time)
- [ ] T041 Test task submission modal integration from lesson page
- [ ] T042 Verify premium lesson task access control
- [ ] T043 Test task display with 0 tasks, 1 task, and multiple tasks
- [ ] T044 Performance test: Measure lesson load time with 10 tasks (<200ms)
- [ ] T045 Verify backward compatibility: lesson API without includeTasks parameter
- [ ] T046 Update API documentation (Swagger) for new endpoints
- [ ] T047 [P] Add XML documentation comments to new DTOs and methods
- [ ] T048 Run all backend tests and ensure >80% coverage
- [ ] T049 Run all frontend tests and ensure >80% coverage
- [ ] T050 Validate against quickstart.md scenarios

---

## Dependencies & Execution Order

### Phase Dependencies

1. **Phase 1 (DTOs)**: No dependencies - can start immediately
2. **Phase 2 (Services)**: Depends on Phase 1 completion
3. **Phase 3 (Controllers)**: Depends on Phase 2 completion
4. **Phase 4 (Backend Tests)**: Depends on Phases 1-3 completion
5. **Phase 5 (Types)**: No dependencies - can start in parallel with Phase 1
6. **Phase 6 (API Services)**: Depends on Phase 5 completion
7. **Phase 7 (Hooks)**: Depends on Phase 6 completion
8. **Phase 8 (Components)**: Depends on Phase 7 completion
9. **Phase 9 (Page Integration)**: Depends on Phase 8 completion
10. **Phase 10 (Sidebar)**: Independent - can run anytime
11. **Phase 11 (Frontend Tests)**: Depends on Phases 5-9 completion
12. **Phase 12 (Integration)**: Depends on all previous phases

### Parallel Opportunities

**Backend Parallel Track** (Phases 1-4):
- T001, T002 (DTOs) can run simultaneously
- T004, T005, T006 (Mappings) can run simultaneously after DTOs
- T015-T019 (Tests) can all run simultaneously once implementation is done

**Frontend Parallel Track** (Phases 5-11):
- Can start simultaneously with backend work
- T020, T021, T022 (Types) can run simultaneously
- T025, T026 (Hooks) can run simultaneously after services
- T035-T038 (Tests) can all run simultaneously once components are done

**Independent Tasks**:
- T034 (Sidebar fix) can be done anytime

### Suggested Execution Strategy

**Option 1: Sequential (Single Developer)**
1. Complete all backend phases (1-4): ~4-6 hours
2. Complete all frontend phases (5-11): ~6-8 hours
3. Integration testing (Phase 12): ~2 hours
**Total: 12-16 hours**

**Option 2: Parallel (2+ Developers)**
- Developer 1: Backend (Phases 1-4)
- Developer 2: Frontend (Phases 5-11)
- Both: Integration (Phase 12)
**Total: ~8-10 hours (wall clock time)**

---

## MVP Scope

**Minimum Viable Product includes**:
- ✅ Backend: DTOs, Services, Controllers (Phases 1-3)
- ✅ Frontend: Types, Services, Hooks, Components (Phases 5-8)
- ✅ Lesson Page Integration (Phase 9)
- ✅ Basic testing (selected tests from Phases 4 & 11)

**Can be deferred to v2**:
- Advanced caching optimizations
- Bulk task operations
- Task filtering/sorting UI
- Comprehensive test coverage (aim for 60% MVP, 80% later)

---

## Independent Test Criteria

### Backend Verification
```bash
# Test lesson with tasks endpoint
curl -H "Authorization: Bearer <token>" \
  "http://localhost:5000/api/lessons/{id}?includeTasks=true"

# Expected: Lesson with tasks array populated

# Test submission status endpoint
curl -H "Authorization: Bearer <token>" \
  "http://localhost:5000/api/tasks/{taskId}/submission-status"

# Expected: Status object or null for never submitted
```

### Frontend Verification
```bash
# Navigate to lesson page
http://localhost:5173/lessons/{lessonId}

# Expected outcomes:
✓ Tasks section appears below video player
✓ Task cards display with correct status badges
✓ Click task opens submission modal
✓ Submit task updates status without page reload
✓ Sidebar "Tasks" link goes to /my-submissions
```

### Integration Verification
1. Login as user
2. Navigate to a lesson with tasks
3. View task list below video
4. Click "Start Task" button
5. Submit task (Quiz/Text/Link/Wallet)
6. Verify status changes to "Pending"
7. Login as admin
8. Approve/Reject task
9. Return as user
10. Verify status updated (may require page refresh or wait for cache invalidation)

---

## Task Checklist Summary

**Total Tasks**: 50
- Phase 1 (DTOs): 6 tasks
- Phase 2 (Services): 6 tasks  
- Phase 3 (Controllers): 2 tasks
- Phase 4 (Backend Tests): 5 tasks
- Phase 5 (Types): 3 tasks
- Phase 6 (API Services): 2 tasks
- Phase 7 (Hooks): 3 tasks
- Phase 8 (Components): 3 tasks
- Phase 9 (Page Integration): 3 tasks
- Phase 10 (Sidebar): 1 task
- Phase 11 (Frontend Tests): 4 tasks
- Phase 12 (Integration): 12 tasks

**Parallel Opportunities**: 20 tasks marked [P]

**Estimated Effort**: 12-16 hours (sequential) or 8-10 hours (parallel)

---

## Implementation Strategy

**Approach**: Incremental delivery following Clean Architecture

1. **Start with Backend Foundation** (Phases 1-3)
   - Ensures API is ready for frontend consumption
   - Allows early API testing with tools like Postman/curl

2. **Build Frontend Incrementally** (Phases 5-9)
   - Start with types and services (testable independently)
   - Add hooks (encapsulate data fetching logic)
   - Build components (reusable UI elements)
   - Integrate into page (complete user experience)

3. **Test Continuously** (Phases 4, 11)
   - Add tests as you build, not at the end
   - Unit tests for services and hooks
   - Integration tests for API endpoints
   - Component tests for UI

4. **Validate End-to-End** (Phase 12)
   - Complete user workflows
   - Performance benchmarks
   - Security validation
   - Documentation updates

**Success Criteria**:
- ✅ Users can see tasks within lesson pages
- ✅ Users can submit tasks from lesson page modal
- ✅ Submission status displays correctly
- ✅ Sidebar navigation works
- ✅ No breaking changes to existing lesson API
- ✅ Performance targets met (<200ms lesson load)
- ✅ Tests pass with >80% coverage
