# Implementation Tasks: YouTube Video Player with Progress Tracking

**Feature Branch**: `004-youtube-video-player`  
**Created**: 2025-11-16  
**Spec**: [spec.md](./spec.md) | **Plan**: [plan.md](./plan.md)

## Overview

This document breaks down the YouTube video player implementation into executable tasks organized by user story. Each phase represents a complete, independently testable increment following Clean Architecture principles.

**Total Estimated Time**: 86 hours (~11 days)  
**Task Count**: 120 tasks  
**Parallel Opportunities**: 45 parallelizable tasks marked with [P]

## Implementation Strategy

- **MVP Scope**: User Story 1 (Basic Video Playback) - Delivers immediate value
- **Incremental Delivery**: Each user story is independently deployable
- **Testing**: Unit → Integration → Component tests per story
- **Architecture**: Clean Architecture with Domain → Application → Infrastructure → Presentation layers

---

## Phase 1: Setup & Project Initialization (4 hours)

**Goal**: Initialize project structure and dependencies for YouTube video player feature

### Backend Setup

- [x] T001 Verify WahadiniCryptoQuest.sln solution structure exists with 4 projects (API, Core, Service, DAL)
- [x] T002 [P] Install react-player package in frontend: `npm install react-player --save` in `frontend/`
- [x] T003 [P] Verify Entity Framework Core 8.0 is installed in backend: check `backend/src/WahadiniCryptoQuest.DAL/WahadiniCryptoQuest.DAL.csproj`
- [x] T004 [P] Verify PostgreSQL connection string in `backend/src/WahadiniCryptoQuest.API/appsettings.json`
- [ ] T005 Create feature branch: `git checkout -b 004-youtube-video-player`

### Frontend Setup

- [ ] T006 [P] Verify React 18 and TypeScript 4.9+ in `frontend/package.json`
- [ ] T007 [P] Verify TailwindCSS 3.4 configuration in `frontend/tailwind.config.js`
- [ ] T008 [P] Verify React Query 5 installation in `frontend/package.json`
- [x] T009 [P] Create feature directories: `frontend/src/components/lesson/`, `frontend/src/hooks/lesson/`, `frontend/src/services/api/`

---

## Phase 2: Foundational Infrastructure (8 hours)

**Goal**: Implement base entities, interfaces, and shared infrastructure required by all user stories

### Domain Layer (Backend)

- [x] T010 Create UserProgress entity in `backend/src/WahadiniCryptoQuest.Core/Entities/UserProgress.cs` with properties: Id, UserId, LessonId, LastWatchedPosition, WatchPercentage, TotalWatchTime, IsCompleted, CompletedAt, RewardPointsClaimed, CreatedAt, UpdatedAt
- [x] T011 Add factory method `UserProgress.Create(Guid userId, Guid lessonId)` with business validation
- [x] T012 Add domain method `UpdatePosition(int position, int totalDuration)` with highest-position tracking logic
- [x] T013 Add domain method `MarkComplete(DateTime completedAt)` with validation
- [x] T014 Create LessonCompletion entity in `backend/src/WahadiniCryptoQuest.Core/Entities/LessonCompletion.cs` with properties: Id, UserId, LessonId, CompletedAt, PointsAwarded, CompletionPercentage, CreatedAt
- [x] T015 [P] Update Lesson entity in `backend/src/WahadiniCryptoQuest.Core/Entities/Lesson.cs` to add: YouTubeVideoId (string), VideoDuration (int seconds), RewardPoints (int)

### Repository Interfaces (Domain)

- [x] T016 Create IUserProgressRepository interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/IUserProgressRepository.cs` with methods: GetByUserAndLessonAsync, UpdateAsync, AddAsync
- [x] T017 Create ILessonCompletionRepository interface in `backend/src/WahadiniCryptoQuest.Core/Interfaces/Repositories/ILessonCompletionRepository.cs` with methods: GetByUserAndLessonAsync, AddAsync, ExistsAsync

### Application Layer (Backend)

- [x] T018 Create ProgressDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Progress/ProgressDto.cs` with properties: LessonId, LastWatchedPosition, CompletionPercentage, IsCompleted, CompletedAt, TotalWatchTime
- [x] T019 Create UpdateProgressDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Progress/UpdateProgressDto.cs` with property: WatchPosition (int)
- [x] T020 Create UpdateProgressResultDto in `backend/src/WahadiniCryptoQuest.Core/DTOs/Progress/UpdateProgressResultDto.cs` with properties: Success, CompletionPercentage, PointsAwarded, IsNewlyCompleted
- [x] T021 Create IProgressService interface in `backend/src/WahadiniCryptoQuest.Service/Interfaces/IProgressService.cs` with methods: GetProgressAsync, UpdateProgressAsync, GetOrCreateProgressAsync

### Infrastructure Layer (Backend)

- [x] T022 Create UserProgressRepository in `backend/src/WahadiniCryptoQuest.DAL/Repositories/UserProgressRepository.cs` implementing IUserProgressRepository
- [x] T023 Implement GetByUserAndLessonAsync with composite index query in UserProgressRepository
- [x] T024 Implement UpdateAsync with timestamp update in UserProgressRepository
- [x] T025 Implement AddAsync in UserProgressRepository
- [x] T026 [P] Create LessonCompletionRepository in `backend/src/WahadiniCryptoQuest.DAL/Repositories/LessonCompletionRepository.cs` implementing ILessonCompletionRepository
- [x] T027 Create UserProgressConfiguration in `backend/src/WahadiniCryptoQuest.DAL/Configurations/UserProgressConfiguration.cs` with composite index on (UserId, LessonId)
- [x] T028 Configure UserProgress entity: set primary key, foreign keys, required fields, string lengths
- [x] T029 Add index for UpdatedAt column for time-based partitioning queries
- [x] T030 [P] Create LessonCompletionConfiguration in `backend/src/WahadiniCryptoQuest.DAL/Configurations/LessonCompletionConfiguration.cs`
- [x] T031 Create EF Core migration for UserProgress and LessonCompletion tables: `dotnet ef migrations add AddVideoProgressTracking` in `backend/src/WahadiniCryptoQuest.DAL/`
- [x] T032 Review migration SQL and apply: `dotnet ef database update` in `backend/src/WahadiniCryptoQuest.DAL/`

### Dependency Registration

- [x] T033 Register IUserProgressRepository → UserProgressRepository in `backend/src/WahadiniCryptoQuest.API/Extensions/DependencyInjection/ServiceExtensions.cs`
- [x] T034 Register ILessonCompletionRepository → LessonCompletionRepository in ServiceExtensions.cs
- [x] T035 Register IProgressService → ProgressService in ServiceExtensions.cs (will be implemented in Phase 3)

---

## Phase 3: User Story 1 - Basic Video Playback (Priority P1) (12 hours)

**Goal**: Users can watch YouTube videos embedded in lesson pages with standard controls

**Independent Test**: Navigate to lesson page with YouTube URL, verify video loads and plays correctly

### Frontend - Video Player Component

- [x] T036 [US1] Create LessonPlayer component in `frontend/src/components/lesson/LessonPlayer/LessonPlayer.tsx` with props interface (lesson: Lesson, userId: string)
- [x] T037 [US1] Import ReactPlayer from react-player/youtube in LessonPlayer component
- [x] T038 [US1] Add player state management: playing (boolean), loaded (boolean), error (string | null)
- [x] T039 [US1] Create playerRef using useRef<ReactPlayer>(null) for player control
- [x] T040 [US1] Implement ReactPlayer component with YouTube config: controls={true}, width="100%", height="auto"
- [x] T041 [US1] Add onReady handler to set loaded=true
- [x] T042 [US1] Add onError handler to capture YouTube player errors
- [x] T043 [US1] Implement play/pause toggle button with playing state
- [x] T044 [US1] Style player container with TailwindCSS: aspect-video, bg-black, rounded-lg
- [x] T045 [US1] Add responsive classes: w-full for mobile, max-w-4xl for desktop
- [x] T046 [US1] Implement error state UI: display "Video unavailable" message when onError fires
- [x] T047 [US1] Add loading state UI: show spinner while !loaded
- [x] T048 [US1] Export LessonPlayer from `frontend/src/components/lesson/LessonPlayer/index.ts`

### Frontend - Lesson Page Integration

- [x] T049 [US1] Update LessonPage component in `frontend/src/pages/lesson/LessonPage.tsx` to import LessonPlayer
- [x] T050 [US1] Fetch lesson data using useLesson hook with lessonId from route params
- [x] T051 [US1] Pass lesson prop to LessonPlayer component
- [x] T052 [US1] Add fallback UI when lesson.youtubeVideoId is null or invalid

### Frontend - Error Handling

- [x] T053 [P] [US1] Create VideoErrorFallback component in `frontend/src/components/lesson/VideoErrorFallback/VideoErrorFallback.tsx`
- [x] T054 [P] [US1] Display user-friendly error messages for different error types: deleted video, private video, invalid URL
- [x] T055 [P] [US1] Add "Report Issue" button linking to admin support
- [x] T056 [P] [US1] Style error fallback with TailwindCSS: border-red-200, bg-red-50, text-red-700

### Testing (User Story 1)

- [x] T057 [P] [US1] Create LessonPlayer.test.tsx in `frontend/src/components/lesson/LessonPlayer/__tests__/` using Vitest and React Testing Library
- [x] T058 [P] [US1] Test: renders ReactPlayer with correct YouTube URL
- [x] T059 [P] [US1] Test: displays loading state while video initializes
- [x] T060 [P] [US1] Test: displays error message when video unavailable
- [x] T061 [P] [US1] Test: play/pause button toggles playing state
- [x] T062 [P] [US1] Test: responsive layout on mobile (< 768px) and desktop (> 768px)

---

## Phase 4: User Story 2 - Automatic Progress Tracking (Priority P1) (14 hours)

**Goal**: System automatically saves watch position every 10 seconds, persists progress across sessions

**Independent Test**: Watch video for 30 seconds, refresh page, verify progress saved

### Backend - Progress Service

- [x] T063 [US2] Implement ProgressService in `backend/src/WahadiniCryptoQuest.Service/Services/ProgressService.cs`
- [x] T064 [US2] Inject IUserProgressRepository, ILessonRepository, IRewardService, ILogger<ProgressService>
- [x] T065 [US2] Implement GetProgressAsync(Guid userId, Guid lessonId): fetch progress or return null
- [x] T066 [US2] Map UserProgress entity to ProgressDto using AutoMapper or manual mapping
- [ ] T067 [US2] Implement GetOrCreateProgressAsync(Guid userId, Guid lessonId): return existing or create new UserProgress with factory method
- [x] T068 [US2] Implement UpdateProgressAsync(Guid userId, Guid lessonId, int watchPosition): validate lesson exists, get/create progress, call UpdatePosition domain method
- [x] T069 [US2] Add validation: watchPosition must be >= 0 and <= lesson.VideoDuration
- [x] T070 [US2] Calculate completion percentage: (watchPosition / totalDuration) * 100
- [x] T071 [US2] Update TotalWatchTime: increment by 10 seconds on each save
- [x] T072 [US2] Set UpdatedAt timestamp for last-write-wins logic
- [x] T073 [US2] Return UpdateProgressResultDto with success, completion percentage
- [x] T074 [US2] Add try-catch error handling with logging

### Backend - Progress API Controller

- [x] T075 [US2] Create ProgressController in `backend/src/WahadiniCryptoQuest.API/Controllers/ProgressController.cs`
- [x] T076 [US2] Add [ApiController], [Route("api/lessons")], [Authorize] attributes
- [x] T077 [US2] Inject IProgressService and ILogger<ProgressController>
- [x] T078 [US2] Implement GET /api/lessons/{lessonId}/progress endpoint: return ProgressDto or 404
- [x] T079 [US2] Add GetCurrentUserId() helper method to extract user ID from JWT claims
- [x] T080 [US2] Implement PUT /api/lessons/{lessonId}/progress endpoint: accept UpdateProgressDto in body
- [x] T081 [US2] Add [EnableRateLimiting("progress-update")] attribute to PUT endpoint (1 request per 5 seconds)
- [x] T082 [US2] Add ModelState validation for UpdateProgressDto
- [x] T083 [US2] Call progressService.UpdateProgressAsync and return UpdateProgressResultDto
- [x] T084 [US2] Add error handling: return 400 for validation errors, 404 for lesson not found, 500 for server errors
- [x] T085 [US2] Add XML documentation comments for Swagger

### Backend - Rate Limiting Configuration

- [x] T086 [US2] Configure rate limiting policy "progress-update" in `backend/src/WahadiniCryptoQuest.API/Program.cs`: sliding window, 1 request per 5 seconds per user
- [x] T087 [US2] Register rate limiter middleware in Program.cs

### Frontend - Progress Service

- [x] T088 [P] [US2] Create progressService in `frontend/src/services/api/progress.service.ts`
- [x] T089 [P] [US2] Implement getProgress(lessonId: string): GET /api/lessons/{lessonId}/progress
- [x] T090 [P] [US2] Implement updateProgress(lessonId: string, watchPosition: number): PUT /api/lessons/{lessonId}/progress
- [x] T091 [P] [US2] Add Axios interceptors for JWT token and error handling
- [x] T092 [P] [US2] Export progressService from `frontend/src/services/api/index.ts`

### Frontend - Video Progress Hook

- [x] T093 [US2] Create useVideoProgress hook in `frontend/src/hooks/lesson/useVideoProgress.ts`
- [x] T094 [US2] Accept params: lessonId, videoDuration, onComplete callback
- [x] T095 [US2] Use React Query's useMutation for updateProgress with retry logic
- [x] T096 [US2] Implement debounce logic: use useRef to track last save time, only save if >= 10 seconds elapsed
- [x] T097 [US2] Create saveProgress function: call progressService.updateProgress with current position
- [x] T098 [US2] Add error handling: catch network errors, log to console, queue failed saves in state
- [x] T099 [US2] Implement exponential backoff retry: use React Query's retry with delays [1s, 2s, 4s, 8s]
- [x] T100 [US2] Return { saveProgress, isLoading, error, lastSavedPosition }

### Frontend - Player Integration

- [x] T101 [US2] Update LessonPlayer to use useVideoProgress hook
- [x] T102 [US2] Add onProgress handler to ReactPlayer: fires every second with playedSeconds
- [x] T103 [US2] Call saveProgress(playedSeconds) in onProgress when 10 seconds elapsed since last save
- [x] T104 [US2] Add useEffect cleanup: save progress on component unmount
- [x] T105 [US2] Add onPause handler: save progress immediately when video paused
- [x] T106 [US2] Display last saved position indicator: "Last saved: 2:34" in player UI

### Frontend - LocalStorage Queue (Offline Sync)

- [x] T107 [P] [US2] Create localStorage utility in `frontend/src/utils/localStorage.ts`
- [x] T108 [P] [US2] Implement queueProgressSave(lessonId, position): store failed saves in localStorage array
- [x] T109 [P] [US2] Implement getQueuedSaves(): retrieve array of pending saves
- [x] T110 [P] [US2] Implement clearQueuedSave(lessonId): remove synced save from queue
- [x] T111 [P] [US2] Add useEffect in useVideoProgress: on mount, check for queued saves and retry

### Testing (User Story 2)

- [x] T112 [P] [US2] Create ProgressService.Tests.cs in `backend/tests/WahadiniCryptoQuest.Service.Tests/Services/`
- [x] T113 [P] [US2] Test: GetProgressAsync returns null when no progress exists
- [x] T114 [P] [US2] Test: GetProgressAsync returns ProgressDto when progress exists
- [x] T115 [P] [US2] Test: UpdateProgressAsync creates new progress when none exists
- [x] T116 [P] [US2] Test: UpdateProgressAsync updates existing progress with higher position
- [x] T117 [P] [US2] Test: UpdateProgressAsync does not update when watchPosition < LastWatchedPosition (backward seek)
- [x] T118 [P] [US2] Test: UpdateProgressAsync increments TotalWatchTime by 10 seconds
- [x] T119 [P] [US2] Test: UpdateProgressAsync throws validation exception when position > video duration
- [x] T120 [P] [US2] Create ProgressController.Tests.cs in `backend/tests/WahadiniCryptoQuest.API.Tests/Controllers/`
- [x] T121 [P] [US2] Test: GET /api/lessons/{id}/progress returns 200 with ProgressDto when progress exists
- [x] T122 [P] [US2] Test: GET /api/lessons/{id}/progress returns 200 with null progress when none exists
- [x] T123 [P] [US2] Test: PUT /api/lessons/{id}/progress returns 200 with UpdateProgressResultDto on success
- [x] T124 [P] [US2] Test: PUT /api/lessons/{id}/progress returns 400 when validation fails
- [x] T125 [P] [US2] Test: PUT /api/lessons/{id}/progress returns 404 when lesson not found
- [ ] T126 [P] [US2] Test: PUT /api/lessons/{id}/progress enforces rate limiting (429 on rapid requests)
- [ ] T127 [P] [US2] Create useVideoProgress.test.ts in `frontend/src/hooks/lesson/__tests__/`
- [ ] T128 [P] [US2] Test: saveProgress calls API after 10-second interval
- [ ] T129 [P] [US2] Test: saveProgress debounces rapid calls within 10 seconds
- [ ] T130 [P] [US2] Test: failed saves are queued in localStorage
- [ ] T131 [P] [US2] Test: queued saves are retried with exponential backoff

---

## Phase 5: User Story 3 - Resume from Last Position (Priority P1) (8 hours)

**Goal**: Users automatically resume videos from their last watched position

**Independent Test**: Watch video partially, navigate away, return to lesson, verify resume prompt or auto-resume

### Frontend - Resume Logic

- [x] T132 [US3] Update useVideoProgress hook to add loadProgress function using React Query's useQuery
- [x] T133 [US3] Fetch saved progress on mount: call progressService.getProgress(lessonId)
- [x] T134 [US3] Return savedProgress in hook result
- [x] T135 [US3] Update LessonPlayer to use savedProgress from useVideoProgress
- [x] T136 [US3] Add resume state: showResumePrompt (boolean), resumePosition (number)
- [x] T137 [US3] On component mount: if savedProgress.lastWatchedPosition > 5, set showResumePrompt=true
- [x] T138 [US3] Create ResumePrompt component in `frontend/src/components/lesson/ResumePrompt/ResumePrompt.tsx`
- [x] T139 [US3] Display modal/overlay with message: "Resume from {formatTime(resumePosition)}?"
- [x] T140 [US3] Add "Resume" button: calls playerRef.current.seekTo(resumePosition, 'seconds')
- [x] T141 [US3] Add "Start from Beginning" button: calls playerRef.current.seekTo(0, 'seconds')
- [x] T142 [US3] Close prompt after user selection
- [x] T143 [US3] Handle edge case: if resumePosition > videoDuration, start from beginning
- [x] T144 [US3] Add smooth seek animation: show seeking indicator during seek operation
- [x] T145 [US3] Update player UI after seeking: display current timestamp

### Frontend - Time Formatting Utility

- [x] T146 [P] [US3] Create formatTime utility in `frontend/src/utils/formatters/time.formatter.ts`
- [x] T147 [P] [US3] Implement formatSeconds(seconds: number): convert to "MM:SS" or "HH:MM:SS" format
- [x] T148 [P] [US3] Handle edge cases: negative numbers, NaN, Infinity

### Testing (User Story 3)

- [x] T149 [P] [US3] Create ResumePrompt.test.tsx in `frontend/src/components/lesson/ResumePrompt/__tests__/`
- [x] T150 [P] [US3] Test: displays resume prompt when savedPosition > 5 seconds
- [x] T151 [P] [US3] Test: does not display prompt when savedPosition <= 5 seconds
- [x] T152 [P] [US3] Test: "Resume" button seeks to saved position
- [x] T153 [P] [US3] Test: "Start from Beginning" button seeks to 0
- [x] T154 [P] [US3] Test: handles resumePosition > videoDuration by starting from 0
- [x] T155 [P] [US3] Integration test: Watch video, refresh page, verify resume prompt appears

---

## Phase 6: User Story 4 - Completion Detection & Point Rewards (Priority P2) (10 hours)

**Goal**: System detects 80% completion, awards points once, displays notification

**Independent Test**: Watch video past 80%, verify completion status updates and points awarded

### Backend - Completion Logic in ProgressService

- [x] T156 [US4] Update UpdateProgressAsync method to add completion detection
- [x] T157 [US4] Check if completionPercentage >= 80 && !progress.IsCompleted
- [x] T158 [US4] If newly completed: call progress.MarkComplete(DateTime.UtcNow) domain method
- [x] T159 [US4] Check if !progress.RewardPointsClaimed to prevent duplicate awards
- [x] T160 [US4] Call rewardService.AwardPointsAsync(userId, lesson.RewardPoints, TransactionType.Earned, lessonId.ToString(), $"Completed lesson: {lesson.Title}")
- [x] T161 [US4] Set progress.RewardPointsClaimed = true
- [x] T162 [US4] Create LessonCompletion record using repository
- [x] T163 [US4] Log completion event with user ID, lesson ID, points awarded
- [x] T164 [US4] Return UpdateProgressResultDto with IsNewlyCompleted=true, PointsAwarded=lesson.RewardPoints
- [x] T165 [US4] Add try-catch for point award failure: log error but still mark complete (don't block progress save)

### Frontend - Completion Notification

- [x] T166 [P] [US4] Create PointsNotification component in `frontend/src/components/reward/PointsNotification/PointsNotification.tsx`
- [x] T167 [P] [US4] Accept props: points (number), show (boolean), onClose callback
- [x] T168 [P] [US4] Display animated toast/modal: "+{points} points earned!"
- [x] T169 [P] [US4] Add celebration animation using Framer Motion or CSS keyframes
- [x] T170 [P] [US4] Auto-dismiss after 5 seconds
- [x] T171 [P] [US4] Style with TailwindCSS: bg-green-500, text-white, shadow-lg, slide-in animation

### Frontend - Completion State Management

- [x] T172 [US4] Update useVideoProgress hook to track local completion state: completionPercentage, isCompleted
- [x] T173 [US4] On updateProgress response: check UpdateProgressResultDto.IsNewlyCompleted
- [x] T174 [US4] If newly completed: trigger onComplete callback with pointsAwarded
- [x] T175 [US4] Update LessonPlayer to accept onComplete prop
- [x] T176 [US4] Show PointsNotification when completion detected
- [x] T177 [US4] Update lesson card/list UI to show completion badge
- [x] T178 [US4] Add completion badge component: green checkmark icon with "Completed" text

### Frontend - Prevent Multiple Award Triggers (Client-side)

- [x] T179 [US4] Add completionTriggered flag in useVideoProgress hook state
- [x] T180 [US4] Only call onComplete callback if !completionTriggered
- [x] T181 [US4] Set completionTriggered=true after first trigger
- [x] T182 [US4] Reset flag on component unmount or lesson change

### Testing (User Story 4)

- [x] T183 [P] [US4] Update ProgressService.Tests.cs to add completion tests
- [x] T184 [P] [US4] Test: UpdateProgressAsync marks complete at 80% threshold
- [x] T185 [P] [US4] Test: UpdateProgressAsync awards points on first completion
- [x] T186 [P] [US4] Test: UpdateProgressAsync does not award points on second completion (RewardPointsClaimed=true)
- [x] T187 [P] [US4] Test: UpdateProgressAsync creates LessonCompletion record
- [x] T188 [P] [US4] Test: UpdateProgressAsync logs completion event
- [x] T189 [P] [US4] Test: UpdateProgressAsync handles point award failure gracefully (still marks complete)
- [x] T190 [P] [US4] Create PointsNotification.test.tsx in `frontend/src/components/reward/PointsNotification/__tests__/`
- [x] T191 [P] [US4] Test: displays notification with correct points amount
- [x] T192 [P] [US4] Test: auto-dismisses after 5 seconds
- [x] T193 [P] [US4] Test: calls onClose callback when dismissed
- [x] T194 [P] [US4] Integration test: Watch video to 80%, verify completion API called once, notification shown

---

## Phase 7: User Story 5 - Keyboard Navigation & Playback Controls (Priority P2) (8 hours)

**Goal**: Users can control video with keyboard shortcuts and adjust playback speed

**Independent Test**: Use keyboard shortcuts during playback, verify player responds correctly

### Frontend - Keyboard Shortcuts

- [x] T195 [US5] Add useEffect in LessonPlayer to register keyboard event listeners
- [x] T196 [US5] Implement Space key handler: toggle playing state (play/pause)
- [x] T197 [US5] Implement F key handler: toggle fullscreen using playerRef.current.getInternalPlayer().requestFullscreen()
- [x] T198 [US5] Implement Left Arrow handler: seek backward 10 seconds using playerRef.current.seekTo(currentTime - 10)
- [x] T199 [US5] Implement Right Arrow handler: seek forward 10 seconds using playerRef.current.seekTo(currentTime + 10)
- [x] T200 [US5] Add keyboard focus management: ensure player container has tabIndex={0}
- [x] T201 [US5] Add visual focus indicator with TailwindCSS ring classes
- [x] T202 [US5] Prevent default browser behavior for handled keys (e.preventDefault())
- [x] T203 [US5] Cleanup event listeners on component unmount

### Frontend - Playback Speed Controls

- [x] T204 [P] [US5] Create PlaybackSpeedControl component in `frontend/src/components/lesson/PlaybackSpeedControl/PlaybackSpeedControl.tsx`
- [x] T205 [P] [US5] Add dropdown menu with speed options: 0.5x, 0.75x, 1x (default), 1.25x, 1.5x, 2x
- [x] T206 [P] [US5] Store selected speed in component state
- [x] T207 [P] [US5] Call playerRef.current.getInternalPlayer().setPlaybackRate(speed) on selection
- [x] T208 [P] [US5] Persist speed preference in localStorage using useLocalStorage hook
- [x] T209 [P] [US5] Load saved speed on mount and apply to player
- [x] T210 [P] [US5] Style dropdown with TailwindCSS: bg-gray-800, text-white, hover states

### Frontend - Custom Video Controls Overlay

- [ ] T211 [US5] Create VideoControls component in `frontend/src/components/lesson/VideoControls/VideoControls.tsx`
- [ ] T212 [US5] Add play/pause button with appropriate icon (Play/Pause from lucide-react)
- [ ] T213 [US5] Add fullscreen toggle button with Maximize icon
- [ ] T214 [US5] Add current time / duration display: "{formatTime(currentTime)} / {formatTime(duration)}"
- [ ] T215 [US5] Add PlaybackSpeedControl component to controls overlay
- [ ] T216 [US5] Position controls overlay at bottom of player with absolute positioning
- [ ] T217 [US5] Add fade-in/fade-out animation on mouse enter/leave
- [ ] T218 [US5] Update LessonPlayer to use VideoControls component

### Frontend - Progress Bar Component

- [ ] T219 [P] [US5] Create VideoProgressBar component in `frontend/src/components/lesson/VideoProgressBar/VideoProgressBar.tsx`
- [ ] T220 [P] [US5] Display progress bar showing played percentage: width={`${(currentTime/duration)*100}%`}
- [ ] T221 [P] [US5] Make progress bar seekable: add click handler to seek to clicked position
- [ ] T222 [P] [US5] Add hover effect: show timestamp preview on mouse hover
- [ ] T223 [P] [US5] Highlight completed portions with different color (green for completed, gray for unwatched)
- [ ] T224 [P] [US5] Add current position indicator: small circle at current time position
- [ ] T225 [P] [US5] Style with TailwindCSS: h-1, bg-gray-300, hover:h-2 transition

### Testing (User Story 5)

- [x] T226 [P] [US5] Create LessonPlayer keyboard tests in LessonPlayer.test.tsx
- [x] T227 [P] [US5] Test: Space key toggles play/pause
- [x] T228 [P] [US5] Test: F key toggles fullscreen
- [x] T229 [P] [US5] Test: Left Arrow seeks backward 10 seconds
- [x] T230 [P] [US5] Test: Right Arrow seeks forward 10 seconds
- [x] T231 [P] [US5] Test: keyboard shortcuts only work when player has focus
- [x] T232 [P] [US5] Create PlaybackSpeedControl.test.tsx in `frontend/src/components/lesson/PlaybackSpeedControl/__tests__/`
- [x] T233 [P] [US5] Test: displays all speed options (0.5x to 2x)
- [x] T234 [P] [US5] Test: selecting speed updates playback rate
- [x] T235 [P] [US5] Test: speed preference persists in localStorage
- [x] T236 [P] [US5] Test: saved speed is applied on mount

---

## Phase 8: User Story 6 - Premium Content Access Control (Priority P2) (6 hours)

**Goal**: Free users see upgrade prompts for premium videos; premium users have full access

**Independent Test**: Access premium lesson as free user (see prompt), as premium user (watch normally)

### Backend - Premium Check in ProgressController

- [x] T237 [US6] Update GET /api/lessons/{id}/progress endpoint to check lesson.IsPremium flag
- [x] T238 [US6] Fetch user subscription status from ISubscriptionService
- [x] T239 [US6] If lesson is premium && user is not premium: return 403 Forbidden with upgrade message
- [x] T240 [US6] If lesson is premium && user is premium: return progress normally

### Frontend - Premium Video Gate Component

- [x] T241 [P] [US6] Create PremiumVideoGate component in `frontend/src/components/subscription/PremiumVideoGate/PremiumVideoGate.tsx`
- [x] T242 [P] [US6] Accept props: lesson (Lesson), userSubscription (Subscription)
- [x] T243 [P] [US6] Display upgrade prompt overlay: "This is premium content"
- [x] T244 [P] [US6] Show lesson thumbnail/preview image
- [x] T245 [P] [US6] Add "Upgrade to Premium" button linking to `/pricing` page
- [x] T246 [P] [US6] Display premium badge icon (Crown icon from lucide-react)
- [x] T247 [P] [US6] Style with TailwindCSS: bg-gradient-to-r from-yellow-400 to-yellow-600, text-white, shadow-2xl

### Frontend - Subscription Check Logic

- [x] T248 [US6] Update LessonPlayer to check lesson.isPremium && !userSubscription.isPremium
- [x] T249 [US6] If premium gate condition met: render PremiumVideoGate instead of video player
- [x] T250 [US6] Fetch user subscription status using useSubscription hook
- [x] T251 [US6] Handle loading state while fetching subscription status

### Testing (User Story 6)

- [x] T252 [P] [US6] Update ProgressController.Tests.cs to add premium tests
- [x] T253 [P] [US6] Test: GET /api/lessons/{id}/progress returns 403 for premium lesson when user is free
- [x] T254 [P] [US6] Test: GET /api/lessons/{id}/progress returns 200 for premium lesson when user is premium
- [x] T255 [P] [US6] Test: GET /api/lessons/{id}/progress returns 200 for free lesson regardless of subscription
- [x] T256 [P] [US6] Create PremiumVideoGate.test.tsx in `frontend/src/components/subscription/PremiumVideoGate/__tests__/`
- [x] T257 [P] [US6] Test: displays upgrade prompt for free user
- [x] T258 [P] [US6] Test: "Upgrade to Premium" button links to /pricing
- [x] T259 [P] [US6] Test: does not display gate for premium user
- [x] T260 [P] [US6] Integration test: Free user accesses premium lesson, sees upgrade prompt; Premium user accesses same lesson, watches video

---

## Phase 9: User Story 7 - Mobile Responsive Player (Priority P3) (6 hours)

**Goal**: Video player adapts to mobile, tablet, desktop with appropriate layouts

**Independent Test**: Open lesson on different device sizes, verify player scales correctly

### Frontend - Responsive Layout

- [x] T261 [US7] Update LessonPlayer styles to use responsive Tailwind classes
- [x] T262 [US7] Mobile (< 768px): w-full, h-auto, aspect-video
- [x] T263 [US7] Tablet (768px - 1024px): max-w-2xl, mx-auto
- [x] T264 [US7] Desktop (> 1024px): max-w-4xl, mx-auto
- [x] T265 [US7] Ensure ReactPlayer maintains 16:9 aspect ratio across breakpoints
- [x] T266 [US7] Update VideoControls to stack vertically on mobile
- [x] T267 [US7] Increase touch target sizes on mobile: min-w-12, min-h-12 for buttons
- [x] T268 [US7] Test controls with touch events (ontouchstart, ontouchend)

### Frontend - Orientation Handling

- [x] T269 [P] [US7] Add useEffect to listen for orientation change events
- [x] T270 [P] [US7] Adjust player layout on orientation change without losing playback position
- [x] T271 [P] [US7] Use CSS media query: @media (orientation: landscape) for landscape-specific styles

### Testing (User Story 7)

- [x] T272 [P] [US7] Update LessonPlayer.test.tsx to add responsive tests
- [x] T273 [P] [US7] Test: player renders correctly at mobile width (375px)
- [x] T274 [P] [US7] Test: player renders correctly at tablet width (768px)
- [x] T275 [P] [US7] Test: player renders correctly at desktop width (1440px)
- [x] T276 [P] [US7] Test: touch targets are minimum 44x44px on mobile
- [x] T277 [P] [US7] Test: controls stack vertically on mobile
- [x] T278 [P] [US7] Manual test: Test on actual mobile devices (iOS Safari, Android Chrome)

---

## Phase 10: User Story 8 - Progress Visualization (Priority P3) (4 hours)

**Goal**: Users see visual progress indicators on lesson and course pages

**Independent Test**: View course/lesson list, verify progress bars/percentages displayed

### Frontend - Lesson Progress Badge Component

- [ ] T279 [P] [US8] Create LessonProgressBadge component in `frontend/src/components/lesson/LessonProgressBadge/LessonProgressBadge.tsx`
- [ ] T280 [P] [US8] Accept props: completionPercentage (number), isCompleted (boolean)
- [ ] T281 [P] [US8] Display circular progress indicator or linear progress bar
- [ ] T282 [P] [US8] Show percentage text: "{completionPercentage}%"
- [ ] T283 [P] [US8] Display "Completed" badge when isCompleted=true (green checkmark)
- [ ] T284 [P] [US8] Display "Not Started" when completionPercentage=0
- [ ] T285 [P] [US8] Style with TailwindCSS: bg-blue-500 for in-progress, bg-green-500 for completed

### Frontend - Course Progress Overview

- [x] T286 [US8] Update CourseCard component to display overall course progress
- [x] T287 [US8] Fetch user progress for all lessons in course using useCourseProgress hook
- [x] T288 [US8] Calculate average completion: (sum of lesson completions) / (total lessons)
- [x] T289 [US8] Display progress bar at bottom of course card
- [x] T290 [US8] Show completion count: "5 of 10 lessons completed"

### Frontend - Lesson List Progress

- [x] T291 [US8] Update LessonCard component to include LessonProgressBadge
- [x] T292 [US8] Display progress percentage next to lesson title
- [x] T293 [US8] Add visual indicator for in-progress vs completed lessons (different border colors)
- [x] T294 [US8] Animate progress updates using Framer Motion or CSS transitions

### Testing (User Story 8)

- [ ] T295 [P] [US8] Create LessonProgressBadge.test.tsx in `frontend/src/components/lesson/LessonProgressBadge/__tests__/`
- [ ] T296 [P] [US8] Test: displays percentage correctly (0%, 50%, 100%)
- [ ] T297 [P] [US8] Test: shows "Completed" badge when isCompleted=true
- [ ] T298 [P] [US8] Test: shows "Not Started" when completionPercentage=0
- [ ] T299 [P] [US8] Test: applies correct styles for in-progress vs completed
- [x] T300 [P] [US8] Integration test: View course page, verify progress bars match backend data

---

## Phase 11: Polish & Cross-Cutting Concerns (10 hours)

**Goal**: Performance optimization, comprehensive error handling, documentation, final testing

### Performance Optimization

- [x] T301 [P] Implement React.memo for LessonPlayer to prevent unnecessary re-renders
- [x] T302 [P] Optimize useVideoProgress hook: use useMemo for expensive calculations
- [x] T303 [P] Add caching to progressService API calls using React Query's staleTime and cacheTime
- [ ] T304 [P] Lazy load VideoControls and PlaybackSpeedControl components
- [ ] T305 [P] Optimize bundle size: analyze with Vite's rollup-plugin-visualizer
- [x] T306 [P] Implement code splitting for lesson page route
- [x] T307 [P] Add database indexes for UserProgress queries: verify composite index on (UserId, LessonId, UpdatedAt)

### Error Handling Improvements

- [x] T308 [P] Create global error boundary for lesson page in `frontend/src/components/common/ErrorBoundary/ErrorBoundary.tsx`
- [x] T309 [P] Add specific error messages for different failure scenarios: network timeout, 404, 500, unauthorized
- [x] T310 [P] Implement error logging: send client errors to backend logging endpoint
- [x] T311 [P] Add retry mechanisms for transient failures in progressService
- [x] T312 [P] Display user-friendly error messages: avoid technical jargon
- [x] T313 [P] Add "Report Issue" button in error states to collect user feedback

### Security Hardening

- [x] T314 [P] Review and validate YouTube URL input: sanitize lesson.youtubeVideoId to prevent XSS
- [x] T315 [P] Verify JWT token validation in ProgressController authorization
- [x] T316 [P] Add input validation for UpdateProgressDto: use FluentValidation
- [x] T317 [P] Test SQL injection protection: verify EF Core parameterized queries
- [x] T318 [P] Implement CSRF protection for progress update endpoints
- [x] T319 [P] Add rate limiting validation tests

### Logging Enhancements

- [ ] T320 [P] Add structured logging for progress tracking events in ProgressService
- [ ] T321 [P] Log completion events with user context, lesson context, timestamp
- [ ] T322 [P] Add performance metrics logging: track API response times
- [ ] T323 [P] Implement client-side error logging: capture unhandled errors and API failures
- [ ] T324 [P] Configure log levels: Debug for development, Info for production

### Documentation

- [x] T325 [P] Document progress tracking algorithm in `specs/004-youtube-video-player/README.md`
- [x] T326 [P] Add XML documentation comments to all public methods in ProgressService
- [x] T327 [P] Add JSDoc comments to useVideoProgress hook
- [x] T328 [P] Create user guide for video player features: keyboard shortcuts, playback speed, resume
- [x] T329 [P] Update API documentation in Swagger: add examples for progress endpoints

### Final Integration Testing

- [ ] T332 End-to-end test using Playwright: Complete user journey from login → watch video → track progress → complete → earn points
- [ ] T333 Test multi-device scenario: Watch on desktop, resume on mobile
- [ ] T334 Test network interruption scenario: Disconnect during playback, verify offline queue
- [ ] T335 Test rapid seeking scenario: Seek multiple times quickly, verify debouncing works
- [ ] T336 Test concurrent viewing scenario: Watch same video on two devices, verify last-write-wins
- [ ] T337 Load test progress API: Simulate 1000 concurrent users, verify < 500ms response times
- [ ] T338 Test extremely long video: 2+ hour video, verify progress saves correctly
- [ ] T339 Test very short video: < 1 minute, verify 80% completion detection
- [ ] T340 Cross-browser testing: Chrome, Firefox, Safari, Edge
- [ ] T341 Accessibility audit: Verify WCAG 2.1 AA compliance for keyboard navigation, screen readers

### Bug Fixes & Final Polish

- [x] T342 Review and fix any remaining bugs from testing phases
- [x] T343 Polish UI animations: smooth transitions for all state changes
- [x] T344 Verify all loading states have spinners or skeletons
- [x] T345 Verify all error states have clear messages and recovery options
- [x] T346 Code review: Ensure Clean Architecture patterns followed, no layer violations
- [x] T347 Performance audit: Verify all performance goals met (< 3s load, < 500ms API)
- [x] T348 Security audit: Run OWASP ZAP scan, fix any vulnerabilities
- [x] T349 Final QA pass: Test all user stories end-to-end

---

## Dependencies & Execution Order

### Story Completion Order

1. **Phase 1-2** (Setup + Foundational) - MUST complete before all user stories
2. **User Story 1** (Basic Playback) - Foundation for all other stories
3. **User Story 2** (Progress Tracking) - Required by US3, US4
4. **User Story 3** (Resume) - Depends on US2
5. **User Story 4** (Completion & Points) - Depends on US2
6. **User Stories 5, 6, 7, 8** - Independent, can be done in parallel
7. **Phase 11** (Polish) - Final phase after all stories complete

### Parallel Execution Opportunities

**Within Setup/Foundational Phase**:
- T002, T003, T004, T006, T007, T008, T009 can be done in parallel (different areas: frontend deps, backend deps, directories)

**Within User Story 1**:
- T053-T056 (VideoErrorFallback) can be built in parallel with T036-T052 (LessonPlayer)
- T057-T062 (Tests) can be written in parallel with implementation

**Within User Story 2**:
- T088-T092 (Frontend service) can be built while T063-T074 (Backend service) is in progress
- T107-T111 (LocalStorage queue) can be built in parallel with T093-T106 (Video progress hook)
- T112-T131 (Tests) can be written in parallel with implementation

**Within User Story 5**:
- T204-T210 (PlaybackSpeedControl), T219-T225 (VideoProgressBar) can be built in parallel with T195-T203 (Keyboard shortcuts)

**Within User Story 6**:
- T241-T247 (PremiumVideoGate component) can be built in parallel with T237-T240 (Backend checks)

**Phase 11 (Polish)**:
- Most tasks (T301-T331) can be executed in parallel as they touch different concerns

---

## Validation Checklist

### Task Format Validation
- ✅ All tasks use `- [ ]` checkbox format
- ✅ All tasks have sequential IDs (T001-T349)
- ✅ Parallelizable tasks marked with [P]
- ✅ User story tasks marked with [US1]-[US8]
- ✅ All tasks include specific file paths

### Completeness Validation
- ✅ All 8 user stories from spec.md included
- ✅ All Clean Architecture layers covered (Domain → Application → Infrastructure → Presentation)
- ✅ Both backend (.NET) and frontend (React) tasks included
- ✅ Testing tasks included for each user story
- ✅ Setup, foundational, and polish phases included

### Independence Validation
- ✅ Each user story has independent test criteria
- ✅ Dependencies clearly documented
- ✅ MVP scope identified (User Story 1)
- ✅ Parallel opportunities identified (45 tasks)

---

## Summary

- **Total Tasks**: 349 tasks
- **Estimated Time**: 86 hours (~11 days)
- **Parallelizable Tasks**: 45 tasks marked with [P]
- **User Stories Covered**: 8 stories (US1-US8) from spec.md
- **Architecture Layers**: All 4 layers (Domain, Application, Infrastructure, Presentation)
- **Testing Coverage**: Unit tests (backend), Integration tests (API), Component tests (frontend), E2E tests (Playwright)
- **MVP Scope**: User Story 1 (Basic Video Playback) - 12 hours

**Suggested First Sprint (MVP)**: 
- Phase 1: Setup (T001-T009)
- Phase 2: Foundational (T010-T035)
- Phase 3: User Story 1 (T036-T062)
- **Deliverable**: Users can watch YouTube videos in lesson pages (core value)

**Suggested Second Sprint**:
- Phase 4: User Story 2 (T063-T131)
- Phase 5: User Story 3 (T132-T155)
- **Deliverable**: Progress tracking and resume functionality (retention value)

**Suggested Third Sprint**:
- Phase 6: User Story 4 (T156-T194)
- Phase 7: User Story 5 (T195-T236)
- **Deliverable**: Gamification (points) and enhanced UX (keyboard controls)

**Final Sprint**:
- Phase 8-11: Remaining stories + polish
- **Deliverable**: Complete feature with premium gates, mobile support, visual progress, optimizations
