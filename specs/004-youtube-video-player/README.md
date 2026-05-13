# YouTube Video Player with Progress Tracking

## Overview

This feature implements a YouTube video player with automatic progress tracking, resume functionality, completion detection, and point rewards for the WahadiniCryptoQuest learning platform.

## Progress Tracking Algorithm

### Core Principles

The progress tracking system follows these fundamental principles:

1. **Highest-Position Tracking**: Only forward progress is saved; backward seeking doesn't decrease position
2. **10-Second Auto-Save**: Progress is automatically saved every 10 seconds during playback
3. **80% Completion Threshold**: Lessons are marked complete at 80% watch time
4. **Last-Write-Wins**: Multi-device conflicts resolved by timestamp (most recent wins)
5. **Point Award Once**: Reward points are awarded only on first completion

### Save Interval Logic

**When progress is saved:**
- Every 10 seconds during active playback (debounced)
- Immediately when user pauses the video
- On component unmount (user navigates away)
- After seeking forward (if position increases)

**When progress is NOT saved:**
- During backward seeking (prevents position decrease)
- Within 10 seconds of last save (debounce window)
- When video is not playing

### Highest-Position Tracking

The system tracks the furthest point a user has reached in a video:

```typescript
// Frontend: useVideoProgress hook
if (newPosition > lastSavedPosition) {
  saveProgress(newPosition); // Only save if moving forward
}
```

```csharp
// Backend: UserProgress entity
public void UpdatePosition(int newPosition, int totalDuration)
{
    // Only update if new position is higher
    if (newPosition > LastWatchedPosition)
    {
        LastWatchedPosition = newPosition;
        CompletionPercentage = CalculateCompletionPercentage(totalDuration);
        TotalWatchTime += 10; // Increment by save interval
        UpdatedAt = DateTime.UtcNow; // For last-write-wins
    }
}
```

**Why highest-position tracking?**
- Prevents accidental position loss from seeking backward
- Allows users to review content without resetting progress
- Accurately reflects actual content consumption

### Completion Detection

**80% Threshold:**

```csharp
if (!progress.IsCompleted && progress.CompletionPercentage >= 80m)
{
    progress.MarkComplete(DateTime.UtcNow);
    // Award points (one-time only)
}
```

**Why 80% instead of 100%?**
- Accounts for credits, outros, and end cards
- Reduces frustration from accidental early exits
- Industry standard for video completion metrics
- Encourages course completion without being overly strict

### Completion Percentage Calculation

```csharp
CompletionPercentage = (decimal)LastWatchedPosition / totalDuration * 100
```

**Example:**
- Video duration: 300 seconds (5 minutes)
- Watch position: 240 seconds (4 minutes)
- Completion: 240 / 300 * 100 = 80%

### Multi-Device Synchronization

**Last-Write-Wins Strategy:**

```csharp
// Each progress update includes UpdatedAt timestamp
UpdatedAt = DateTime.UtcNow;

// When loading progress:
var mostRecent = progressRecords.OrderByDescending(p => p.UpdatedAt).First();
```

**Conflict Resolution Example:**
1. User watches on desktop to 2:00 at 10:00 AM
2. User watches on mobile to 1:30 at 10:05 AM
3. System keeps mobile's 1:30 position (most recent)
4. Desktop loads: fetches 1:30 from server
5. If user seeks forward on desktop to 2:30, new record is saved

### Offline Queue & Retry Logic

**Queue Management:**

```typescript
// Frontend: localStorage queue
const progressQueue: Array<{
  lessonId: string;
  position: number;
  timestamp: string;
}> = [];

// On network failure:
queueProgressSave(lessonId, position);

// On network recovery:
const queued = getQueuedSaves();
for (const save of queued) {
  await retryWithBackoff(() => saveProgress(save));
}
```

**Exponential Backoff:**
- Retry 1: 1 second delay
- Retry 2: 2 seconds delay
- Retry 3: 4 seconds delay
- Retry 4: 8 seconds delay
- After 4 failures: Queue for later sync

### Point Award Prevention

**Duplicate Prevention:**

```csharp
if (!progress.IsCompleted && progress.CompletionPercentage >= 80m)
{
    progress.MarkComplete(DateTime.UtcNow);
    
    // Check flag to prevent duplicate awards
    if (!progress.RewardPointsClaimed)
    {
        var points = currentLesson.RewardPoints;
        await _rewardService.AwardPointsAsync(userId, points);
        progress.ClaimRewardPoints(); // Set flag
        
        // Create completion record for audit trail
        var completion = LessonCompletion.Create(userId, lessonId, points);
        await _lessonCompletionRepository.AddAsync(completion);
    }
}
```

**Why this approach?**
- Prevents gaming the system by re-watching
- Maintains audit trail in LessonCompletion table
- Idempotent: Safe to call multiple times
- Flag persists across sessions

## Database Schema

### UserProgress Table

```sql
CREATE TABLE user_progress (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    lesson_id UUID NOT NULL REFERENCES lessons(id),
    last_watched_position INT NOT NULL DEFAULT 0, -- Seconds
    completion_percentage DECIMAL(5,2) NOT NULL DEFAULT 0,
    total_watch_time INT NOT NULL DEFAULT 0, -- Total seconds watched
    is_completed BOOLEAN NOT NULL DEFAULT FALSE,
    completed_at TIMESTAMP NULL,
    reward_points_claimed BOOLEAN NOT NULL DEFAULT FALSE,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    -- Composite unique index for fast lookups
    CONSTRAINT uk_user_lesson UNIQUE (user_id, lesson_id)
);

-- Index for time-based queries (last-write-wins)
CREATE INDEX idx_user_progress_updated_at ON user_progress(updated_at);

-- Index for completion queries
CREATE INDEX idx_user_progress_completed ON user_progress(is_completed);
```

### LessonCompletion Table

```sql
CREATE TABLE lesson_completions (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL REFERENCES users(id),
    lesson_id UUID NOT NULL REFERENCES lessons(id),
    completed_at TIMESTAMP NOT NULL DEFAULT NOW(),
    points_awarded INT NOT NULL DEFAULT 0,
    completion_percentage DECIMAL(5,2) NOT NULL,
    created_at TIMESTAMP NOT NULL DEFAULT NOW(),
    
    -- Ensure one completion record per user-lesson
    CONSTRAINT uk_user_lesson_completion UNIQUE (user_id, lesson_id)
);
```

## API Endpoints

### GET /api/lessons/{lessonId}/progress

Retrieves current progress for a lesson.

**Response:**
```json
{
  "lessonId": "uuid",
  "lastWatchedPosition": 240,
  "completionPercentage": 80.0,
  "isCompleted": true,
  "completedAt": "2025-11-16T10:30:00Z",
  "totalWatchTime": 300
}
```

### PUT /api/lessons/{lessonId}/progress

Updates watch progress.

**Request:**
```json
{
  "watchPosition": 250
}
```

**Response:**
```json
{
  "success": true,
  "completionPercentage": 83.3,
  "pointsAwarded": 100,
  "isNewlyCompleted": true
}
```

**Rate Limiting:** 1 request per 5 seconds per user (prevents spam)

## Frontend Implementation

### useVideoProgress Hook

```typescript
export function useVideoProgress({ 
  lessonId, 
  videoDuration, 
  onComplete 
}) {
  const [lastSaveTime, setLastSaveTime] = useState(0);
  const [completionTriggered, setCompletionTriggered] = useState(false);
  
  const saveProgress = useCallback((position: number) => {
    const now = Date.now();
    
    // Debounce: Only save if 10 seconds elapsed
    if (now - lastSaveTime < 10000) {
      return;
    }
    
    // Call API
    updateProgressMutation.mutate({ 
      lessonId, 
      watchPosition: position 
    });
    
    setLastSaveTime(now);
  }, [lastSaveTime, lessonId]);
  
  // ... rest of hook
}
```

### Resume Functionality

```typescript
// On component mount:
if (savedProgress?.lastWatchedPosition > 5) {
  setShowResumePrompt(true);
}

// On resume:
playerRef.current.seekTo(savedProgress.lastWatchedPosition, 'seconds');
```

**Why 5-second threshold?**
- Ignores accidental clicks or very short views
- Reduces unnecessary prompts
- Improves user experience

## Performance Considerations

### Caching Strategy

**Frontend (React Query):**
```typescript
useQuery({
  queryKey: ['progress', lessonId],
  queryFn: () => getProgress(lessonId),
  staleTime: 2 * 60 * 1000, // 2 minutes
  gcTime: 5 * 60 * 1000, // 5 minutes
});
```

**Backend:**
- Database composite index on (user_id, lesson_id) for O(1) lookups
- UpdatedAt index for efficient last-write-wins queries
- Connection pooling (10-100 connections)

### Rate Limiting

- **Progress updates:** 1 request / 5 seconds / user
- **Prevents:** API abuse, database overload
- **Implementation:** Sliding window rate limiter in ASP.NET Core

## Error Handling

### Network Failures

1. **Immediate retry** with exponential backoff (1s, 2s, 4s, 8s)
2. **Queue failed saves** in localStorage
3. **Sync on reconnection** by processing queue
4. **User feedback:** Toast notification "Progress will sync when online"

### Validation Errors

```csharp
[FluentValidation]
public class UpdateProgressValidator : AbstractValidator<UpdateProgressDto>
{
    RuleFor(x => x.WatchPosition)
        .GreaterThanOrEqualTo(0)
        .WithMessage("Watch position must be >= 0");
}
```

### Conflict Resolution

- **Strategy:** Last-write-wins based on UpdatedAt timestamp
- **Trade-off:** May lose older progress if devices used simultaneously
- **Mitigation:** Highest-position tracking prevents going backward

## Security

### Input Sanitization

```typescript
// YouTube video ID validation (XSS prevention)
const YOUTUBE_VIDEO_ID_REGEX = /^[a-zA-Z0-9_-]{11}$/;

if (!sanitizeYouTubeVideoId(lesson.youtubeVideoId)) {
  return <InvalidVideoError />;
}
```

### Authorization

- **JWT bearer tokens:** Required for all progress endpoints
- **User isolation:** UserId from JWT claims, no user can access others' progress
- **Premium content:** Verified on GET endpoint (403 if unauthorized)

### SQL Injection

- **Protection:** Entity Framework Core parameterized queries
- **No raw SQL:** All queries use LINQ expressions
- **Safe by default:** EF Core prevents injection automatically

## Testing

### Unit Tests

- `ProgressService.Tests.cs`: 10+ tests covering all scenarios
- `useVideoProgress.test.ts`: 8+ tests for frontend logic
- Coverage: >90% for core progress tracking code

### Integration Tests

- `ProgressController.Tests.cs`: API endpoint testing
- Database round-trips verified
- Rate limiting enforcement tested

### E2E Tests (Planned)

- Complete user journey: watch → pause → resume → complete
- Multi-device scenario testing
- Network interruption recovery

## Monitoring & Logging

### Structured Logging

```csharp
_logger.LogInformation(
    "Progress update: UserId={UserId}, LessonId={LessonId}, " +
    "CompletionPercentage={CompletionPercentage}%, " +
    "Duration={DurationMs}ms",
    userId, lessonId, completionPercentage, duration);
```

### Log Levels

**Development:**
- Debug: All progress saves, completions, errors
- Info: Lesson completions, point awards
- Warning: Invalid data, missing lessons
- Error: Exceptions, database failures

**Production:**
- Info: Completions, point awards only
- Warning: Validation errors, missing data
- Error: Exceptions, critical failures

### Metrics to Track

- **Average completion time:** How long to reach 80%?
- **Completion rate:** % of started lessons completed
- **Resume rate:** % of users who resume vs restart
- **API response times:** Should be < 500ms p95
- **Error rate:** Should be < 0.1% of requests

## Troubleshooting

### Progress not saving

**Symptoms:** User watches video but progress doesn't persist

**Causes:**
1. Network connectivity issues → Check offline queue
2. Rate limiting triggered → Slow down save frequency
3. Invalid lesson ID → Check database

**Solution:**
```typescript
// Check localStorage queue
const queued = localStorage.getItem('progress_queue');
console.log('Queued saves:', JSON.parse(queued));

// Check error logs
console.log('Progress errors:', progressErrors);
```

### Resume not working

**Symptoms:** Progress exists but resume prompt doesn't show

**Causes:**
1. Position < 5 seconds → Threshold not met
2. Position > duration → Invalid data
3. Cached data stale → Force refresh

**Solution:**
```typescript
// Force refresh progress
queryClient.invalidateQueries(['progress', lessonId]);
```

### Points not awarded

**Symptoms:** Lesson marked complete but no points

**Causes:**
1. RewardPointsClaimed flag already set
2. RewardService integration missing
3. Lesson has 0 reward points

**Solution:**
```csharp
// Check flags
SELECT reward_points_claimed, is_completed 
FROM user_progress 
WHERE user_id = 'xxx' AND lesson_id = 'yyy';

// Check completion record
SELECT * FROM lesson_completions 
WHERE user_id = 'xxx' AND lesson_id = 'yyy';
```

## Future Enhancements

### Planned Features

1. **Watch history analytics:** Detailed viewing patterns
2. **Progress streaks:** Consecutive days of learning
3. **Video bookmarks:** Save specific timestamps
4. **Playback speed tracking:** Correlate speed with completion
5. **Engagement metrics:** Pause frequency, rewind count

### Performance Optimizations

1. **Database partitioning:** Time-based partitions for user_progress
2. **Read replicas:** Separate read/write databases
3. **CDN caching:** Cache lesson metadata
4. **WebSocket updates:** Real-time progress sync across devices

## References

- [React Player Documentation](https://github.com/cookpete/react-player)
- [YouTube IFrame API](https://developers.google.com/youtube/iframe_api_reference)
- [Entity Framework Core Best Practices](https://learn.microsoft.com/en-us/ef/core/)
- [ASP.NET Core Rate Limiting](https://learn.microsoft.com/en-us/aspnet/core/performance/rate-limit)

## Change Log

### 2025-11-16 - Initial Implementation
- ✅ Basic video playback with YouTube embed
- ✅ Automatic progress tracking (10-second intervals)
- ✅ Resume from last position
- ✅ 80% completion detection
- ✅ Point rewards on completion
- ✅ Multi-device synchronization
- ✅ Offline queue with retry logic
- ✅ Mobile responsive design
- ✅ Keyboard shortcuts
- ✅ Premium content gating
- ✅ Comprehensive error handling
- ✅ Security hardening (XSS, SQL injection, CSRF protection)
- ✅ Structured logging with performance metrics

---

**Maintained by:** WahadiniCryptoQuest Platform Team  
**Last Updated:** November 16, 2025  
**Version:** 1.0.0
