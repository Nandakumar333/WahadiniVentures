# Implementation Plan: YouTube Video Player with Progress Tracking

**Branch**: `004-youtube-video-player` | **Date**: 2025-11-16 | **Spec**: [spec.md](./spec.md)
**Input**: Feature specification from `/specs/004-youtube-video-player/spec.md`

## Summary

Implement a robust YouTube video player with automatic progress tracking (10-second intervals), resume functionality, completion detection (80% threshold), and point rewards integration. The feature uses react-player for YouTube embedding, Clean Architecture with separate domain entities (UserProgress, LessonCompletion), application services (ProgressService), and RESTful API endpoints. Key technical aspects include exponential backoff retry logic, localStorage queue for offline sync, highest-position tracking for skip-ahead behavior, and multi-device synchronization via last-write-wins strategy.

## Technical Context

**Language/Version**: 
- Backend: .NET 8 C# with ASP.NET Core Web API
- Frontend: TypeScript 4.9+ with React 18

**Primary Dependencies**: 
- Backend: Entity Framework Core 8.0, AutoMapper, FluentValidation, MediatR, Serilog, JWT Bearer tokens
- Frontend: react-player (YouTube), React Router 7, React Query 5, Zustand, React Hook Form 7, Zod, Axios, TailwindCSS 3.4

**Storage**: PostgreSQL 15+ with JSONB support, time-based partitioning for user activity data

**Testing**: 
- Backend: xUnit with Moq for unit tests, WebApplicationFactory for integration tests
- Frontend: Vitest for unit tests, React Testing Library for component tests, Playwright for E2E tests

**Target Platform**: Web application (desktop, tablet, mobile responsive)

**Project Type**: Web application with backend API and React frontend

**Performance Goals**: 
- Video loading < 3 seconds average
- Progress save API < 500ms response time
- Support 1000+ concurrent video watchers
- Maximum 6 database writes per minute per user (debounced)

**Constraints**: 
- 99%+ progress save reliability
- Resume within 2 seconds of page load
- Point rewards within 1 second of completion
- 200ms keyboard shortcut response time

**Scale/Scope**: 
- 10,000+ progress save operations per hour during peak
- Handle extremely long videos (2+ hours)
- Support multi-device concurrent viewing
- Mobile-responsive breakpoints: 768px (tablet), 480px (mobile)

## Constitution Check

*GATE: Must pass before Phase 0 research. Re-check after Phase 1 design.*

**Learning-First Experience**: ✅ PASS
- Progress tracking focuses on comprehension through 80% completion threshold (not just clicking "complete")
- Point rewards motivate completion but don't distract from content (silent background operation)
- Resume functionality prevents frustration and encourages continued learning
- Highest-position tracking allows users to skip known content without penalties
- Total watch time analytics enable tracking genuine engagement vs. gaming

**Security & Privacy Standards**: ✅ PASS
- All API endpoints require JWT authentication
- User watch history and progress data is private and user-specific
- Input validation prevents malicious YouTube URLs and XSS attacks
- Rate limiting on progress update endpoint (1 call per 5 seconds max)
- Audit logging for point awards and completion events
- No third-party sharing of watch history data

**Scalability & Performance**: ✅ PASS
- Progress tracking uses debounced saves (10-second intervals) to minimize database writes
- Video playback relies on YouTube's CDN (no bandwidth strain on platform)
- Progress data uses indexed queries (UserId + LessonId composite index)
- Supports 1000+ concurrent users watching with < 500ms API response times
- Mobile-responsive design with proper breakpoints (768px, 480px)
- Exponential backoff retry prevents thundering herd on failures

**Fair Gamification Economy**: ✅ PASS
- Point awards occur only once per video completion (RewardPointsClaimed flag)
- 80% completion threshold requires meaningful engagement
- Points cannot be earned unlimited times by re-watching
- Transaction logging creates immutable audit trail for fraud detection
- Future enhancement planned for actual watch-time verification to prevent skip-to-end farming
- Total watch time tracked for analytics and fraud detection

**Content Quality Assurance**: ✅ PASS
- Videos must be validated before assignment to lessons (admin workflow)
- Broken/unavailable videos display clear error messages
- System preserves user progress even when video becomes unavailable
- Error handling prevents crashes and provides recovery guidance
- Admin dashboard alerts for unavailable videos

**Accessibility & Transparency**: ✅ PASS
- Keyboard shortcuts (Space, F, arrows) enable navigation without mouse
- Screen readers can access player controls (ARIA labels required)
- Video captions/subtitles supported through YouTube native features
- Color contrast meets WCAG 2.1 AA standards for player controls
- Touch targets on mobile minimum 44x44px
- Clear error messages without technical jargon

**Business Model Ethics**: ✅ PASS
- Free users can access free content without restrictions
- Premium content gates are transparent with clear upgrade prompts
- No misleading "Play" buttons on premium content for free users
- User progress data not sold to third parties
- Users can delete watch history (privacy controls)
- Premium video access based on subscription status, not arbitrary limits

**Technical Excellence**: ✅ PASS
- Clean Architecture: Domain (UserProgress, LessonCompletion), Application (ProgressService), Infrastructure (Repository), Presentation (ProgressController)
- Comprehensive error handling prevents crashes
- Test coverage planned: unit tests for ProgressService, integration tests for API endpoints, component tests for LessonPlayer
- CI/CD pipeline validates functionality
- Code follows .NET and TypeScript best practices
- Proper separation of concerns with custom hooks (useVideoProgress)

## Project Structure

### Documentation (this feature)

```text
specs/004-youtube-video-player/
├── spec.md              # Feature specification (completed)
├── plan.md              # This file (implementation plan)
├── research.md          # Phase 0: Technical decisions and patterns
├── data-model.md        # Phase 1: Entity and database design
├── quickstart.md        # Phase 1: Developer onboarding guide
├── contracts/           # Phase 1: API contracts and DTOs
│   ├── progress-api.md
│   └── dtos.md
└── checklists/
    └── requirements.md  # Specification quality checklist (completed)
```

### Source Code (repository root)

```text
backend/
├── src/
│   ├── WahadiniCryptoQuest.API/
│   │   ├── Controllers/
│   │   │   └── ProgressController.cs          # NEW: Progress tracking endpoints
│   │   ├── Extensions/
│   │   │   └── DependencyInjection/
│   │   │       └── ServiceExtensions.cs       # MODIFY: Register progress services
│   │   └── Program.cs                          # MODIFY: Add rate limiting config
│   │
│   ├── WahadiniCryptoQuest.Core/
│   │   ├── Entities/
│   │   │   ├── UserProgress.cs                 # NEW: Progress tracking entity
│   │   │   ├── LessonCompletion.cs             # NEW: Completion tracking entity
│   │   │   └── Lesson.cs                       # MODIFY: Add video-related fields
│   │   ├── DTOs/
│   │   │   └── Progress/
│   │   │       ├── ProgressDto.cs              # NEW: Progress response DTO
│   │   │       ├── UpdateProgressDto.cs        # NEW: Progress update request
│   │   │       └── UpdateProgressResultDto.cs  # NEW: Progress update response
│   │   └── Interfaces/
│   │       ├── Services/
│   │       │   └── IProgressService.cs         # NEW: Progress service interface
│   │       └── Repositories/
│   │           ├── IProgressRepository.cs      # NEW: Progress data access
│   │           └── ILessonCompletionRepository.cs # NEW: Completion data access
│   │
│   ├── WahadiniCryptoQuest.Service/
│   │   ├── Services/
│   │   │   └── ProgressService.cs              # NEW: Progress business logic
│   │   └── Mappings/
│   │       └── ProgressMappingProfile.cs       # NEW: AutoMapper profile
│   │
│   └── WahadiniCryptoQuest.DAL/
│       ├── Context/
│       │   └── ApplicationDbContext.cs         # MODIFY: Add DbSets for entities
│       ├── Repositories/
│       │   ├── ProgressRepository.cs           # NEW: Progress repository
│       │   └── LessonCompletionRepository.cs   # NEW: Completion repository
│       ├── Configurations/
│       │   ├── UserProgressConfiguration.cs    # NEW: EF Core configuration
│       │   └── LessonCompletionConfiguration.cs # NEW: EF Core configuration
│       └── Migrations/
│           └── 20251116_AddVideoProgressTracking.cs # NEW: Database migration
│
└── tests/
    ├── WahadiniCryptoQuest.API.Tests/
    │   └── Controllers/
    │       └── ProgressControllerTests.cs      # NEW: Controller tests
    ├── WahadiniCryptoQuest.Service.Tests/
    │   └── Services/
    │       └── ProgressServiceTests.cs         # NEW: Service unit tests
    └── WahadiniCryptoQuest.DAL.Tests/
        └── Repositories/
            └── ProgressRepositoryTests.cs      # NEW: Repository tests

frontend/
├── src/
│   ├── components/
│   │   └── lesson/
│   │       ├── LessonPlayer.tsx                # NEW: Main video player component
│   │       ├── VideoControls.tsx               # NEW: Player controls overlay
│   │       ├── ProgressBar.tsx                 # NEW: Visual progress indicator
│   │       ├── PremiumVideoGate.tsx            # NEW: Access control component
│   │       └── ResumePrompt.tsx                # NEW: Resume dialog
│   ├── hooks/
│   │   ├── useVideoProgress.ts                 # NEW: Progress tracking hook
│   │   ├── useVideoPlayer.ts                   # NEW: Player state management
│   │   └── useLocalStorageQueue.ts             # NEW: Offline save queue
│   ├── services/
│   │   ├── api/
│   │   │   └── progress.service.ts             # NEW: Progress API client
│   │   └── storage/
│   │       └── progressQueue.service.ts        # NEW: LocalStorage management
│   ├── types/
│   │   └── progress.types.ts                   # NEW: TypeScript interfaces
│   └── utils/
│       ├── youtubeHelpers.ts                   # NEW: URL parsing utilities
│       └── retryHelpers.ts                     # NEW: Exponential backoff logic
│
└── tests/
    ├── components/
    │   └── lesson/
    │       └── LessonPlayer.test.tsx           # NEW: Component tests
    └── hooks/
        └── useVideoProgress.test.ts            # NEW: Hook tests
```

**Structure Decision**: Web application structure with Clean Architecture. Backend follows Domain-Application-Infrastructure-Presentation layers with Entity Framework Core for data access. Frontend uses feature-based component organization with custom hooks for business logic separation. Test directories mirror source structure.

## Complexity Tracking

> No Constitution violations requiring justification.

---

## Phase 0: Research & Technical Decisions

### Research Tasks

1. **React Player YouTube Integration Patterns**
   - Decision: Use react-player/youtube lightweight import for bundle optimization
   - Pattern: PlayerRef for programmatic control, onProgress callback for tracking
   - State management: Separate player state from business logic via custom hooks

2. **Progress Tracking Debounce Strategy**
   - Decision: Combination of interval-based (10s) and event-based (pause, unmount) saves
   - Pattern: useRef for interval management, cleanup on unmount
   - Debounce library: Native setTimeout/clearTimeout (no lodash dependency)

3. **Exponential Backoff Retry Implementation**
   - Decision: Custom retry utility with configurable backoff: 1s, 2s, 4s, 8s intervals
   - Pattern: Recursive async function with delay, max retry count of 4 attempts
   - Storage: Failed requests queued in localStorage with timestamp

4. **LocalStorage Queue Synchronization**
   - Decision: Custom queue service with batch sync on network recovery
   - Pattern: Store failed requests as JSON array, process FIFO on reconnection
   - Conflict resolution: Server timestamp wins, discard stale local updates

5. **Multi-Device Race Condition Handling**
   - Decision: Server-side last-write-wins using LastUpdatedAt timestamp comparison
   - Pattern: Client sends timestamp with each update, server compares before save
   - Optimization: No client-side conflict resolution (server authoritative)

6. **Highest Position Tracking Logic**
   - Decision: Compare incoming position vs saved position, only update if higher
   - Pattern: Backend service logic, not client responsibility
   - Edge case: Allow backward seeks for navigation without losing progress

7. **Keyboard Shortcut Implementation**
   - Decision: Global keydown listener with player focus check
   - Pattern: useEffect cleanup, prevent default on handled keys
   - Accessibility: Don't capture when input/textarea has focus

8. **Rate Limiting Strategy**
   - Decision: ASP.NET Core rate limiting middleware with sliding window
   - Configuration: 1 request per 5 seconds per user per endpoint
   - Fallback: Client-side debounce prevents hitting limit

9. **Premium Content Access Control**
   - Decision: Server-side authorization before serving video data
   - Pattern: Check subscription status in API, return 403 if unauthorized
   - Frontend: Conditional rendering based on user.subscription.isPremium

10. **Mobile Responsive Player Design**
    - Decision: TailwindCSS responsive utilities with aspect-ratio CSS
    - Breakpoints: 768px (tablet), 480px (mobile)
    - Touch: Native YouTube controls handle touch events

### Technology Choices

| Technology | Purpose | Rationale |
|------------|---------|-----------|
| react-player | YouTube embedding | Most popular React video player, 24k+ stars, excellent YouTube support |
| React Query | Server state caching | Handles progress fetching with automatic cache invalidation |
| Zustand | Player state | Lightweight alternative to Redux for local player state |
| localStorage | Offline queue | Browser-native, no dependencies, synchronous access |
| EF Core Migrations | Database schema | Code-first approach with versioning and rollback support |
| FluentValidation | Input validation | Declarative validation rules separate from controller logic |
| Rate Limiting Middleware | API protection | Built-in ASP.NET Core feature, no external dependencies |

---

## Phase 1: Design & Contracts

### Deliverables
- `data-model.md` - Entity design with relationships and validation rules
- `contracts/` - API contracts with request/response DTOs
- `quickstart.md` - Developer setup and testing guide
- Updated copilot-instructions.md with new technologies

### Tasks

#### 1.1 Domain Entity Design (2 hours)

**Entities to Create/Modify:**

**UserProgress** (NEW)
```csharp
public class UserProgress
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public int LastWatchedPosition { get; private set; } // seconds
    public decimal WatchPercentage { get; private set; } // 0-100
    public int TotalWatchTime { get; private set; } // seconds
    public bool IsCompleted { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public bool RewardPointsClaimed { get; private set; }
    public DateTime LastUpdatedAt { get; private set; }
    public DateTime CreatedAt { get; private set; }
    
    // Navigation properties
    public virtual User User { get; set; }
    public virtual Lesson Lesson { get; set; }
    
    // Factory method
    public static UserProgress Create(Guid userId, Guid lessonId)
    
    // Domain methods
    public void UpdatePosition(int positionSeconds, int videoDurationSeconds)
    public void MarkComplete(int pointsValue)
    public void ClaimReward()
}
```

**LessonCompletion** (NEW)
```csharp
public class LessonCompletion
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public Guid LessonId { get; private set; }
    public DateTime CompletedAt { get; private set; }
    public int PointsAwarded { get; private set; }
    public decimal CompletionPercentage { get; private set; }
    
    // Navigation properties
    public virtual User User { get; set; }
    public virtual Lesson Lesson { get; set; }
    
    // Factory method
    public static LessonCompletion Create(Guid userId, Guid lessonId, int points)
}
```

**Lesson** (MODIFY - Add fields)
```csharp
// Add to existing Lesson entity:
public string? YouTubeVideoUrl { get; private set; }
public int? VideoDurationSeconds { get; private set; }
public int RewardPoints { get; private set; }

// New navigation properties:
public virtual ICollection<UserProgress> UserProgresses { get; set; }
public virtual ICollection<LessonCompletion> Completions { get; set; }

// New methods:
public void SetVideoUrl(string youtubeUrl)
public void SetVideoDuration(int durationSeconds)
```

#### 1.2 Repository Interfaces (1 hour)

**IProgressRepository**
```csharp
public interface IProgressRepository : IRepository<UserProgress>
{
    Task<UserProgress?> GetByUserAndLessonAsync(Guid userId, Guid lessonId);
    Task<IEnumerable<UserProgress>> GetUserProgressAsync(Guid userId);
    Task<IEnumerable<UserProgress>> GetInProgressAsync(Guid userId);
    Task<IEnumerable<UserProgress>> GetCompletedAsync(Guid userId);
}
```

**ILessonCompletionRepository**
```csharp
public interface ILessonCompletionRepository : IRepository<LessonCompletion>
{
    Task<IEnumerable<LessonCompletion>> GetUserCompletionsAsync(Guid userId);
    Task<bool> HasCompletedAsync(Guid userId, Guid lessonId);
}
```

#### 1.3 API Contract Design (2 hours)

**Endpoints:**

```
GET    /api/lessons/{lessonId}/progress
PUT    /api/lessons/{lessonId}/progress
POST   /api/lessons/{lessonId}/complete (optional manual completion)
```

**DTOs:**

**ProgressDto** (Response)
```csharp
public class ProgressDto
{
    public Guid LessonId { get; set; }
    public int LastWatchedPosition { get; set; }
    public decimal CompletionPercentage { get; set; }
    public bool IsCompleted { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int TotalWatchTime { get; set; }
}
```

**UpdateProgressDto** (Request)
```csharp
public class UpdateProgressDto
{
    [Required]
    [Range(0, int.MaxValue)]
    public int WatchPosition { get; set; }
}
```

**UpdateProgressResultDto** (Response)
```csharp
public class UpdateProgressResultDto
{
    public bool Success { get; set; }
    public decimal CompletionPercentage { get; set; }
    public int PointsAwarded { get; set; }
    public bool IsNewlyCompleted { get; set; }
    public string? Message { get; set; }
}
```

#### 1.4 Database Configuration (2 hours)

**EF Core Configuration:**

```csharp
// UserProgressConfiguration.cs
public class UserProgressConfiguration : IEntityTypeConfiguration<UserProgress>
{
    public void Configure(EntityTypeBuilder<UserProgress> builder)
    {
        builder.HasKey(p => p.Id);
        
        builder.HasIndex(p => new { p.UserId, p.LessonId })
               .IsUnique();
               
        builder.HasIndex(p => p.LastUpdatedAt);
        
        builder.Property(p => p.WatchPercentage)
               .HasPrecision(5, 2);
               
        builder.HasOne(p => p.User)
               .WithMany()
               .HasForeignKey(p => p.UserId)
               .OnDelete(DeleteBehavior.Cascade);
               
        builder.HasOne(p => p.Lesson)
               .WithMany(l => l.UserProgresses)
               .HasForeignKey(p => p.LessonId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
```

**Migration Plan:**
- Add UserProgress table with indexes
- Add LessonCompletion table with indexes
- Add fields to Lesson table (YouTubeVideoUrl, VideoDurationSeconds, RewardPoints)
- No data migration required (new feature)

#### 1.5 Frontend Type Definitions (1 hour)

**progress.types.ts**
```typescript
export interface Progress {
  lessonId: string;
  lastWatchedPosition: number;
  completionPercentage: number;
  isCompleted: boolean;
  completedAt?: string;
  totalWatchTime: number;
}

export interface UpdateProgressRequest {
  watchPosition: number;
}

export interface UpdateProgressResponse {
  success: boolean;
  completionPercentage: number;
  pointsAwarded: number;
  isNewlyCompleted: boolean;
  message?: string;
}

export interface VideoPlayerState {
  playing: boolean;
  progress: number;
  currentTime: number;
  duration: number;
  lastSavedPosition: number;
  isCompleted: boolean;
  playbackRate: number;
}

export interface ProgressQueueItem {
  lessonId: string;
  watchPosition: number;
  timestamp: number;
  retryCount: number;
}
```

---

## Phase 2: Implementation Breakdown

### Phase 2.1: Backend Domain & Repository Layer (8 hours)

**Tasks:**
1. Create UserProgress entity with factory methods and domain logic (2h)
2. Create LessonCompletion entity (1h)
3. Modify Lesson entity to add video fields (1h)
4. Create EF Core configurations for new entities (1h)
5. Implement IProgressRepository with queries (2h)
6. Implement ILessonCompletionRepository (1h)

**Deliverables:**
- Domain entities with private setters and factory methods
- Repository implementations with indexed queries
- EF Core configurations with proper relationships
- Database migration file

**Testing:**
- Unit tests for entity factory methods and domain logic
- Repository integration tests with in-memory database

### Phase 2.2: Backend Application Layer (10 hours)

**Tasks:**
1. Create IProgressService interface (1h)
2. Implement ProgressService with GetOrCreateProgressAsync (2h)
3. Implement UpdateProgressAsync with completion detection (3h)
4. Implement GetProgressAsync (1h)
5. Add point award integration with IRewardService (2h)
6. Create AutoMapper profile for DTOs (1h)

**Key Logic - UpdateProgressAsync:**
```csharp
public async Task<UpdateProgressResultDto> UpdateProgressAsync(
    Guid userId, Guid lessonId, int watchPosition)
{
    var progress = await GetOrCreateProgressAsync(userId, lessonId);
    var lesson = await _lessonRepository.GetByIdAsync(lessonId);
    
    // Validate watch position
    if (watchPosition < 0 || watchPosition > lesson.VideoDurationSeconds)
        throw new ValidationException("Invalid watch position");
    
    // Update position (highest position tracking)
    if (watchPosition > progress.LastWatchedPosition)
    {
        progress.UpdatePosition(watchPosition, lesson.VideoDurationSeconds);
    }
    
    // Increment total watch time
    progress.TotalWatchTime += 10;
    
    // Check for completion (80% threshold)
    bool isNewlyCompleted = false;
    int pointsAwarded = 0;
    
    if (progress.WatchPercentage >= 80 && !progress.IsCompleted)
    {
        progress.MarkComplete(lesson.RewardPoints);
        
        // Award points
        if (!progress.RewardPointsClaimed)
        {
            await _rewardService.AwardPointsAsync(
                userId, lesson.RewardPoints, 
                TransactionType.Earned, 
                lessonId.ToString(), 
                $"Completed lesson: {lesson.Title}");
            
            progress.ClaimReward();
            pointsAwarded = lesson.RewardPoints;
            isNewlyCompleted = true;
            
            // Create completion record
            var completion = LessonCompletion.Create(userId, lessonId, pointsAwarded);
            await _lessonCompletionRepository.AddAsync(completion);
        }
    }
    
    await _progressRepository.UpdateAsync(progress);
    await _unitOfWork.CommitAsync();
    
    return new UpdateProgressResultDto
    {
        Success = true,
        CompletionPercentage = progress.WatchPercentage,
        PointsAwarded = pointsAwarded,
        IsNewlyCompleted = isNewlyCompleted
    };
}
```

**Deliverables:**
- ProgressService with all business logic
- Integration with existing RewardService
- Comprehensive error handling
- Logging for key operations

**Testing:**
- Unit tests for ProgressService methods
- Mock IRewardService for point award tests
- Test completion detection at 80% threshold
- Test duplicate point award prevention

### Phase 2.3: Backend API Layer (6 hours)

**Tasks:**
1. Create ProgressController with dependency injection (1h)
2. Implement GET /api/lessons/{id}/progress endpoint (1h)
3. Implement PUT /api/lessons/{id}/progress endpoint (2h)
4. Add FluentValidation for UpdateProgressDto (1h)
5. Configure rate limiting middleware (1h)
6. Add Swagger documentation (included in implementation)

**Rate Limiting Configuration:**
```csharp
// Program.cs
builder.Services.AddRateLimiter(options =>
{
    options.AddSlidingWindowLimiter("progress-update", options =>
    {
        options.Window = TimeSpan.FromSeconds(5);
        options.PermitLimit = 1;
        options.QueueLimit = 0;
        options.SegmentsPerWindow = 1;
    });
});
```

**Deliverables:**
- ProgressController with 2 endpoints (GET, PUT)
- Validation and error handling
- Rate limiting configuration
- Swagger/OpenAPI documentation

**Testing:**
- Integration tests for API endpoints
- Test authentication requirements
- Test rate limiting behavior
- Test validation errors

### Phase 2.4: Frontend Video Player Component (12 hours)

**Tasks:**
1. Install and configure react-player (1h)
2. Create LessonPlayer component structure (2h)
3. Implement useVideoPlayer hook for state management (2h)
4. Add keyboard shortcut handlers (2h)
5. Implement playback speed controls (1h)
6. Style player with TailwindCSS (2h)
7. Make responsive for mobile (2h)

**Key Component Structure:**
```tsx
// LessonPlayer.tsx
export const LessonPlayer: React.FC<LessonPlayerProps> = ({ lesson }) => {
  const playerRef = useRef<ReactPlayer>(null);
  const [playing, setPlaying] = useState(false);
  const [playbackRate, setPlaybackRate] = useState(1);
  
  const { progress, updateProgress, isLoading } = useVideoProgress(lesson.id);
  
  // Auto-save interval
  useEffect(() => {
    if (playing) {
      const interval = setInterval(saveProgress, 10000);
      return () => clearInterval(interval);
    }
  }, [playing]);
  
  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) return;
      
      switch (e.key) {
        case ' ':
          e.preventDefault();
          setPlaying(prev => !prev);
          break;
        case 'f':
        case 'F':
          e.preventDefault();
          if (document.fullscreenElement) {
            document.exitFullscreen();
          } else {
            playerRef.current?.wrapper?.requestFullscreen();
          }
          break;
        case 'ArrowLeft':
          playerRef.current?.seekTo(playerRef.current.getCurrentTime() - 10, 'seconds');
          break;
        case 'ArrowRight':
          playerRef.current?.seekTo(playerRef.current.getCurrentTime() + 10, 'seconds');
          break;
      }
    };
    
    window.addEventListener('keydown', handleKeyPress);
    return () => window.removeEventListener('keydown', handleKeyPress);
  }, []);
  
  const saveProgress = async () => {
    if (!playerRef.current) return;
    const currentTime = Math.floor(playerRef.current.getCurrentTime());
    if (currentTime > 0) {
      await updateProgress(currentTime);
    }
  };
  
  return (
    <div className="relative aspect-video bg-black rounded-lg overflow-hidden">
      <ReactPlayer
        ref={playerRef}
        url={lesson.youtubeVideoUrl}
        playing={playing}
        playbackRate={playbackRate}
        width="100%"
        height="100%"
        onEnded={() => setPlaying(false)}
        config={{
          youtube: {
            playerVars: { showinfo: 1 }
          }
        }}
      />
      <VideoControls
        playing={playing}
        playbackRate={playbackRate}
        onPlayPause={() => setPlaying(prev => !prev)}
        onPlaybackRateChange={setPlaybackRate}
      />
    </div>
  );
};
```

**Deliverables:**
- LessonPlayer component with react-player integration
- Keyboard shortcut support
- Playback speed controls
- Responsive design with TailwindCSS
- Video controls overlay

**Testing:**
- Component tests for player rendering
- Test keyboard shortcuts
- Test playback speed changes
- Visual regression tests for responsive design

### Phase 2.5: Frontend Progress Tracking Logic (10 hours)

**Tasks:**
1. Create useVideoProgress custom hook (3h)
2. Implement progress API service (1h)
3. Add retry logic with exponential backoff (2h)
4. Implement localStorage queue service (2h)
5. Add progress load on mount (1h)
6. Implement debounced save mechanism (1h)

**useVideoProgress Hook:**
```typescript
export const useVideoProgress = (lessonId: string) => {
  const [progress, setProgress] = useState<Progress | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const saveQueue = useRef<ProgressQueueItem[]>([]);
  
  // Load progress on mount
  useEffect(() => {
    const loadProgress = async () => {
      try {
        const data = await progressService.getProgress(lessonId);
        setProgress(data);
      } catch (error) {
        console.error('Failed to load progress:', error);
      } finally {
        setIsLoading(false);
      }
    };
    
    loadProgress();
  }, [lessonId]);
  
  // Process queued saves on mount
  useEffect(() => {
    processQueue();
  }, []);
  
  const updateProgress = async (watchPosition: number) => {
    try {
      const result = await retryWithBackoff(
        () => progressService.updateProgress(lessonId, watchPosition),
        { maxRetries: 4, baseDelay: 1000 }
      );
      
      setProgress(prev => prev ? {
        ...prev,
        lastWatchedPosition: watchPosition,
        completionPercentage: result.completionPercentage,
        isCompleted: result.isNewlyCompleted || prev.isCompleted
      } : null);
      
      if (result.isNewlyCompleted && result.pointsAwarded > 0) {
        toast.success(`+${result.pointsAwarded} points! Lesson completed! 🎉`);
      }
      
      return result;
    } catch (error) {
      // Queue failed save for later
      queueSave({ lessonId, watchPosition, timestamp: Date.now(), retryCount: 0 });
      throw error;
    }
  };
  
  const processQueue = async () => {
    const queue = getQueuedSaves();
    if (queue.length === 0) return;
    
    for (const item of queue) {
      try {
        await progressService.updateProgress(item.lessonId, item.watchPosition);
        removeFromQueue(item);
      } catch (error) {
        // Keep in queue, will retry next time
      }
    }
  };
  
  return {
    progress,
    isLoading,
    updateProgress
  };
};
```

**Retry Utility:**
```typescript
export const retryWithBackoff = async <T>(
  fn: () => Promise<T>,
  options: { maxRetries: number; baseDelay: number }
): Promise<T> => {
  let lastError: Error;
  
  for (let i = 0; i <= options.maxRetries; i++) {
    try {
      return await fn();
    } catch (error) {
      lastError = error as Error;
      if (i < options.maxRetries) {
        const delay = options.baseDelay * Math.pow(2, i);
        await new Promise(resolve => setTimeout(resolve, delay));
      }
    }
  }
  
  throw lastError!;
};
```

**Deliverables:**
- useVideoProgress hook with all tracking logic
- Retry utility with exponential backoff
- localStorage queue service
- Debounced save mechanism
- Progress load functionality

**Testing:**
- Unit tests for useVideoProgress hook
- Test retry logic with failing API calls
- Test localStorage queue operations
- Test debounce behavior

### Phase 2.6: Frontend Resume & Completion Features (8 hours)

**Tasks:**
1. Create ResumePrompt component (2h)
2. Implement resume functionality (2h)
3. Create ProgressBar component (2h)
4. Add point award notifications (1h)
5. Implement completion detection UI (1h)

**ResumePrompt Component:**
```tsx
export const ResumePrompt: React.FC<ResumePromptProps> = ({
  lastPosition,
  onResume,
  onStartOver
}) => {
  const formatTime = (seconds: number) => {
    const mins = Math.floor(seconds / 60);
    const secs = seconds % 60;
    return `${mins}:${secs.toString().padStart(2, '0')}`;
  };
  
  return (
    <div className="absolute inset-0 flex items-center justify-center bg-black/70 z-50">
      <div className="bg-white rounded-lg p-6 max-w-md">
        <h3 className="text-xl font-bold mb-4">Welcome back!</h3>
        <p className="text-gray-600 mb-6">
          You last watched up to {formatTime(lastPosition)}. 
          Would you like to resume or start over?
        </p>
        <div className="flex gap-4">
          <Button onClick={onResume} variant="primary" className="flex-1">
            Resume from {formatTime(lastPosition)}
          </Button>
          <Button onClick={onStartOver} variant="outline" className="flex-1">
            Start from beginning
          </Button>
        </div>
      </div>
    </div>
  );
};
```

**Deliverables:**
- ResumePrompt component with UI
- Resume functionality in LessonPlayer
- ProgressBar visual component
- Toast notifications for completion
- Point award feedback

**Testing:**
- Component tests for ResumePrompt
- Test resume vs. start over behavior
- Test ProgressBar rendering
- Test notification triggers

### Phase 2.7: Frontend Premium Content & Error Handling (6 hours)

**Tasks:**
1. Create PremiumVideoGate component (2h)
2. Implement subscription check logic (1h)
3. Add error handling for unavailable videos (2h)
4. Add loading states (1h)

**PremiumVideoGate Component:**
```tsx
export const PremiumVideoGate: React.FC<PremiumVideoGateProps> = ({
  lesson,
  onUpgrade
}) => {
  return (
    <div className="relative aspect-video bg-gradient-to-br from-yellow-500 to-orange-600 rounded-lg overflow-hidden flex items-center justify-center">
      <div className="text-center text-white p-8">
        <Crown className="w-16 h-16 mx-auto mb-4" />
        <h3 className="text-2xl font-bold mb-2">Premium Content</h3>
        <p className="mb-6 opacity-90">
          Upgrade to Premium to access this lesson and unlock all premium content
        </p>
        <Button 
          onClick={onUpgrade} 
          variant="secondary"
          className="bg-white text-orange-600 hover:bg-gray-100"
        >
          Upgrade to Premium
        </Button>
      </div>
    </div>
  );
};
```

**Deliverables:**
- PremiumVideoGate component
- Subscription status checking
- Error handling for unavailable videos
- Loading states and skeletons

**Testing:**
- Test premium gate rendering
- Test subscription checks
- Test error scenarios (404, deleted videos)
- Test loading state displays

### Phase 2.8: Testing & Polish (8 hours)

**Tasks:**
1. Write backend unit tests (3h)
2. Write backend integration tests (2h)
3. Write frontend component tests (2h)
4. Cross-browser testing (1h)

**Testing Coverage:**

Backend:
- ProgressService unit tests (factory methods, completion logic, point awards)
- Repository integration tests (queries, unique constraints)
- API controller integration tests (authentication, validation, rate limiting)

Frontend:
- LessonPlayer component tests (rendering, keyboard shortcuts)
- useVideoProgress hook tests (API calls, retry logic, queue)
- ResumePrompt component tests (user interactions)
- Integration tests with mocked API

**Deliverables:**
- 80%+ backend test coverage
- 70%+ frontend test coverage
- All edge cases tested
- E2E tests for critical paths

---

## Phase 3: Deployment & Documentation

### Phase 3.1: Database Migration (2 hours)

**Tasks:**
1. Generate EF Core migration
2. Review migration SQL
3. Test migration on dev database
4. Prepare rollback script

**Migration:**
```bash
cd backend
dotnet ef migrations add AddVideoProgressTracking --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API
dotnet ef database update --project src/WahadiniCryptoQuest.DAL --startup-project src/WahadiniCryptoQuest.API
```

### Phase 3.2: Documentation (2 hours)

**Tasks:**
1. Update API documentation (Swagger)
2. Write developer guide in quickstart.md
3. Document keyboard shortcuts for users
4. Update README with new feature

**Deliverables:**
- API documentation
- Developer onboarding guide
- User-facing keyboard shortcut guide
- Updated README

### Phase 3.3: Performance Testing (2 hours)

**Tasks:**
1. Load test progress API (1000+ concurrent users)
2. Test video player on slow connections
3. Verify mobile responsiveness
4. Measure bundle size impact

**Metrics to Verify:**
- API response time < 500ms (p95)
- Video load time < 3 seconds
- Mobile performance score > 90
- Bundle size increase < 50KB

---

## Timeline Estimate

| Phase | Tasks | Estimated Hours |
|-------|-------|----------------|
| Phase 0: Research | Technical decisions, patterns | 4h |
| Phase 1: Design | Data model, contracts, docs | 8h |
| Phase 2.1: Backend Domain | Entities, repositories | 8h |
| Phase 2.2: Backend Application | Services, business logic | 10h |
| Phase 2.3: Backend API | Controllers, validation | 6h |
| Phase 2.4: Frontend Player | Component, keyboard shortcuts | 12h |
| Phase 2.5: Frontend Tracking | Progress hook, retry, queue | 10h |
| Phase 2.6: Frontend Resume | Resume prompt, completion UI | 8h |
| Phase 2.7: Frontend Premium | Access gates, error handling | 6h |
| Phase 2.8: Testing | Backend + frontend tests | 8h |
| Phase 3: Deployment | Migration, docs, performance | 6h |
| **TOTAL** | | **86 hours (~11 days)** |

---

## Success Criteria Validation

| Criterion | Target | Validation Method |
|-----------|--------|-------------------|
| SC-001: Video load time | < 3 seconds | Performance monitoring |
| SC-002: Progress save reliability | 99%+ | Error rate monitoring |
| SC-003: Resume load time | < 2 seconds | Performance tests |
| SC-004: Point reward speed | < 1 second | Integration tests |
| SC-005: Point award success rate | 95%+ | Transaction logs |
| SC-006: Mobile functionality | All devices | Manual testing |
| SC-007: Premium gate enforcement | 100% | Integration tests |
| SC-008: Keyboard shortcut response | < 200ms | User testing |
| SC-009: Error message display | 100% | Error scenario tests |
| SC-010: Concurrent user support | 1000+ | Load testing |
| SC-011: Database write optimization | ≤ 6/min/user | Query monitoring |
| SC-012: Completion rate increase | +25% | Analytics comparison |
| SC-013: First video completion | 90%+ | User analytics |
| SC-014: Watch time increase | +30% | Analytics comparison |

---

## Risk Assessment

| Risk | Impact | Mitigation |
|------|--------|----------|
| YouTube API changes | High | Use stable react-player library with active maintenance |
| Race conditions on multi-device | Medium | Server-side last-write-wins with timestamp validation |
| LocalStorage quota exceeded | Low | Implement queue size limit (max 50 items), FIFO eviction |
| Rate limiting too strict | Medium | Client-side debounce prevents hitting limit, adjust if needed |
| Video duration changes | Low | Handle gracefully by comparing timestamp to current duration |
| Network instability | Medium | Exponential backoff retry + localStorage queue |
| High database write volume | Medium | Debouncing reduces writes to max 6/min/user |

---

## Next Steps

1. ✅ Review and approve this implementation plan
2. Generate detailed task breakdown with `/speckit.tasks`
3. Begin Phase 0 research and create `research.md`
4. Complete Phase 1 design artifacts (`data-model.md`, `contracts/`, `quickstart.md`)
5. Start implementation with Phase 2.1 (Backend Domain Layer)

**Command to proceed**: `/speckit.tasks`