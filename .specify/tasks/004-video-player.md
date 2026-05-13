# Feature: YouTube Video Player & Progress Tracking

## /speckit.specify

### Feature Overview
Implement a robust YouTube video player with automatic progress tracking, resume functionality, completion detection, and point rewards. This is a core feature that enables users to watch educational content and track their learning progress.

### Feature Scope
**Included:**
- YouTube video player using react-player library
- Automatic progress tracking (save position every 10 seconds)
- Resume from last watched position
- Completion detection at 80% threshold
- Automatic point awards on completion
- Premium content access gates
- Keyboard shortcuts (Space, F, arrows)
- Playback speed controls (0.5x, 1x, 1.5x, 2x)
- Video quality selection
- Handle unavailable videos gracefully
- Progress visualization
- Mobile responsive player

**Excluded:**
- Video hosting (YouTube only)
- Custom video player UI (using YouTube's controls)
- Video downloads
- Picture-in-picture (future enhancement)
- Comments/notes on videos (future)
- Video recommendations (future)

### User Stories
1. As a user, I want to watch lesson videos so I can learn crypto concepts
2. As a user, I want my watch position saved automatically so I don't lose my place
3. As a user, I want to resume from where I left off when I return
4. As a user, I want to earn points when I complete videos
5. As a user, I want keyboard shortcuts for efficient video control
6. As a user, I want to adjust playback speed to learn at my pace
7. As a premium user, I want access to premium video content
8. As a free user, I want to see what premium videos offer
9. As a user, I want to see my progress through the video
10. As a user, I want clear error messages if a video is unavailable

### Technical Requirements
- Frontend: React 18, TypeScript, react-player library
- Backend: .NET 8 C#, ASP.NET Core Web API
- Database: PostgreSQL (UserProgress table)
- State Management: React hooks for player state
- API Integration: RESTful endpoints for progress
- Performance: Debounced save operations
- Error Handling: Graceful degradation for video errors
- Responsive: Works on desktop, tablet, mobile

---

## /speckit.plan

### Implementation Plan

#### Phase 1: Backend Progress Service (4 hours)
**Tasks:**
1. Review/enhance UserProgress entity
2. Create IProgressService interface
3. Implement ProgressService with business logic
4. Add progress calculation methods
5. Implement completion detection logic
6. Add point award integration

**Deliverables:**
- ProgressService with methods for update, get, complete
- Progress validation logic
- Integration with RewardService for point awards

#### Phase 2: Backend API Endpoints (3 hours)
**Tasks:**
1. Create ProgressController
2. Implement PUT /api/lessons/{id}/progress endpoint
3. Implement GET /api/lessons/{id}/progress endpoint
4. Implement POST /api/lessons/{id}/complete endpoint
5. Add validation and error handling
6. Configure authorization

**Deliverables:**
- ProgressController with 3 endpoints
- DTOs for progress updates
- Swagger documentation

#### Phase 3: Frontend Video Player Component (6 hours)
**Tasks:**
1. Install and configure react-player
2. Create LessonPlayer component
3. Implement video controls integration
4. Add progress tracking logic (10-second intervals)
5. Implement auto-save mechanism with debouncing
6. Add keyboard shortcut handlers
7. Style player with TailwindCSS

**Deliverables:**
- LessonPlayer.tsx component
- useVideoProgress custom hook
- Player controls overlay

#### Phase 4: Progress Tracking & Resume (4 hours)
**Tasks:**
1. Implement progress load on component mount
2. Add resume from last position functionality
3. Create completion detection (80% threshold)
4. Implement point award trigger
5. Add progress visualization
6. Handle page navigation/refresh gracefully

**Deliverables:**
- Resume functionality
- Completion detection
- Progress bar component
- Point award notifications

#### Phase 5: Premium Content Gates (2 hours)
**Tasks:**
1. Check user subscription before loading video
2. Create PremiumVideoGate component
3. Show upgrade prompt for non-premium users
4. Add preview functionality for premium content

**Deliverables:**
- PremiumVideoGate component
- Subscription check integration
- Upgrade prompt UI

#### Phase 6: Error Handling & Polish (3 hours)
**Tasks:**
1. Handle deleted/unavailable videos
2. Add loading states
3. Implement playback speed controls
4. Add quality selection
5. Test on various devices
6. Optimize performance

**Deliverables:**
- Error handling for all scenarios
- Polished UI/UX
- Mobile optimizations

---

## /speckit.clarify

### Questions & Answers

**Q: What happens if a user skips ahead in the video?**
A: We track the highest watched position. Skipping forward updates progress, but we could add fraud detection in future if needed.

**Q: How do we handle users who reload the page mid-video?**
A: Progress is saved every 10 seconds to backend. On reload, we fetch last position and offer to resume.

**Q: What if YouTube video is deleted or made private?**
A: Display error message: "Video unavailable". Admin can update video ID. User progress is preserved.

**Q: Should we prevent users from earning points by skipping to the end?**
A: For MVP, we allow it. Future enhancement: require watching at least 80% of video duration (not just seeking to 80%).

**Q: How precise should progress tracking be?**
A: Save position every 10 seconds. This balances accuracy with API call frequency.

**Q: What if user watches on multiple devices?**
A: Use last updated position. If user continues on device B after watching on device A, they can resume from device A's position.

**Q: Should we show a "Mark as complete" button?**
A: No, completion is automatic at 80%. This ensures users actually watch content.

**Q: How do we handle very short videos (< 2 minutes)?**
A: Completion threshold applies: 80% of any duration. A 1-minute video needs 48 seconds watched.

**Q: Can users replay videos after completion?**
A: Yes, they can rewatch anytime. Progress updates but points are only awarded once.

**Q: What about network interruptions during save?**
A: Implement retry logic with exponential backoff. Queue failed saves in localStorage for later sync.

**Q: Should we track watch time for analytics?**
A: Yes, store total watch time in UserProgress for analytics purposes.

**Q: How to handle concurrent viewing sessions?**
A: Last write wins. If user watches same video on two devices simultaneously, latest progress update is saved.

---

## /speckit.analyze

### Technical Architecture

#### Backend Structure
```
WahadiniCryptoQuest.Application/
├── Interfaces/
│   └── IProgressService.cs
├── Services/
│   └── ProgressService.cs
└── DTOs/
    ├── ProgressDto.cs
    ├── UpdateProgressDto.cs
    └── CompletionDto.cs

WahadiniCryptoQuest.API/
└── Controllers/
    └── ProgressController.cs
```

#### Frontend Structure
```
src/
├── components/
│   └── lesson/
│       ├── LessonPlayer.tsx (main player component)
│       ├── VideoControls.tsx
│       ├── ProgressBar.tsx
│       └── PremiumVideoGate.tsx
├── hooks/
│   ├── useVideoProgress.ts (custom hook for progress tracking)
│   └── useVideoPlayer.ts (player state management)
├── services/
│   └── progressService.ts (API integration)
└── types/
    └── progress.types.ts
```

#### API Endpoints

```
GET /api/lessons/{lessonId}/progress
    Response: {
      lessonId: string,
      lastWatchedPosition: number (seconds),
      completionPercentage: number,
      isCompleted: boolean,
      completedAt: string?,
      totalWatchTime: number
    }

PUT /api/lessons/{lessonId}/progress
    Body: {
      watchPosition: number (seconds)
    }
    Response: {
      success: boolean,
      completionPercentage: number,
      pointsAwarded: number (if newly completed)
    }

POST /api/lessons/{lessonId}/complete
    Response: {
      success: boolean,
      pointsAwarded: number,
      message: string
    }
```

#### State Management

```typescript
interface VideoPlayerState {
  playing: boolean;
  progress: number; // 0-100
  currentTime: number; // seconds
  duration: number; // seconds
  lastSavedPosition: number;
  isCompleted: boolean;
  playbackRate: number;
  quality: string;
}

interface ProgressHook {
  progress: ProgressDto | null;
  loading: boolean;
  error: string | null;
  updateProgress: (position: number) => Promise<void>;
  completeLesson: () => Promise<number>; // returns points awarded
}
```

#### Progress Tracking Flow

```
1. User navigates to LessonPage
2. LessonPlayer component mounts
3. useVideoProgress hook loads saved progress
4. If saved position > 30s, show "Resume from X:XX?" prompt
5. User clicks resume or starts from beginning
6. Video plays
7. Every 10 seconds while playing:
   - Get current time from player
   - Call updateProgress(currentTime) (debounced)
   - Backend updates UserProgress.LastWatchedPosition
8. When progress reaches 80%:
   - Backend marks IsCompleted = true
   - Backend awards RewardPoints via RewardService
   - Frontend shows "+X points" notification
9. User can rewatch, but points only awarded once
```

#### Completion Detection Logic

```csharp
public async Task<(bool IsNewlyCompleted, int PointsAwarded)> UpdateProgressAsync(
    Guid userId, Guid lessonId, int watchPosition)
{
    var progress = await GetOrCreateProgressAsync(userId, lessonId);
    var lesson = await _lessonRepository.GetByIdAsync(lessonId);
    
    // Update position
    progress.LastWatchedPosition = watchPosition;
    progress.TotalWatchTime += 10; // Add interval
    
    // Calculate completion percentage
    var completionPercentage = (watchPosition / (double)lesson.Duration) * 100;
    progress.CompletionPercentage = (decimal)completionPercentage;
    
    // Check for completion (80% threshold)
    bool isNewlyCompleted = false;
    int pointsAwarded = 0;
    
    if (completionPercentage >= 80 && !progress.IsCompleted)
    {
        progress.IsCompleted = true;
        progress.CompletedAt = DateTime.UtcNow;
        
        // Award points
        if (!progress.RewardPointsClaimed)
        {
            await _rewardService.AwardPointsAsync(
                userId, lesson.RewardPoints, 
                TransactionType.Earned, 
                lessonId.ToString(), 
                $"Completed lesson: {lesson.Title}");
            
            progress.RewardPointsClaimed = true;
            pointsAwarded = lesson.RewardPoints;
            isNewlyCompleted = true;
        }
    }
    
    await _progressRepository.UpdateAsync(progress);
    return (isNewlyCompleted, pointsAwarded);
}
```

#### Security Measures
1. Validate user has access to lesson before loading progress
2. Check premium status for premium lessons
3. Rate limit progress update endpoint (max 1 call per 5 seconds per user)
4. Validate watch position is within video duration
5. Prevent duplicate point awards (RewardPointsClaimed flag)
6. Validate lesson exists before updating progress

---

## /speckit.checklist

### Implementation Checklist

#### Backend Setup
- [ ] Review UserProgress entity (ensure all fields present)
- [ ] Add TotalWatchTime field if missing
- [ ] Create IProgressService interface
- [ ] Implement ProgressService
- [ ] Add GetOrCreateProgressAsync method
- [ ] Add UpdateProgressAsync method
- [ ] Add GetProgressAsync method
- [ ] Add CompleteManuallyAsync method (optional)
- [ ] Integrate with IRewardService

#### Progress Service Methods
- [ ] GetOrCreateProgressAsync implementation
- [ ] UpdateProgressAsync with completion detection
- [ ] Calculate completion percentage
- [ ] Award points on completion
- [ ] Prevent duplicate point awards
- [ ] Handle edge cases (zero duration, etc.)
- [ ] Add logging for progress updates

#### API Controller
- [ ] Create ProgressController
- [ ] Implement GET /api/lessons/{id}/progress
- [ ] Implement PUT /api/lessons/{id}/progress
- [ ] Implement POST /api/lessons/{id}/complete
- [ ] Add authorization attributes
- [ ] Add validation
- [ ] Add rate limiting
- [ ] Create DTOs (ProgressDto, UpdateProgressDto)
- [ ] Add Swagger documentation

#### Frontend Video Player
- [ ] Install react-player package
- [ ] Create LessonPlayer component
- [ ] Set up react-player instance
- [ ] Add ref for player control
- [ ] Implement play/pause toggle
- [ ] Add onProgress handler
- [ ] Add onEnded handler
- [ ] Add keyboard shortcuts (Space, F, arrows)
- [ ] Style player container
- [ ] Make responsive for mobile

#### Progress Tracking
- [ ] Create useVideoProgress custom hook
- [ ] Load saved progress on mount
- [ ] Implement auto-save every 10 seconds
- [ ] Debounce progress updates
- [ ] Save progress on unmount
- [ ] Save progress on pause
- [ ] Handle save errors gracefully
- [ ] Queue failed saves for retry

#### Resume Functionality
- [ ] Display resume prompt if position > 30s
- [ ] Seek to saved position on resume
- [ ] Allow starting from beginning option
- [ ] Animate seek on resume
- [ ] Update UI after seeking

#### Completion Detection
- [ ] Track completion percentage locally
- [ ] Detect 80% threshold
- [ ] Trigger point award API call
- [ ] Display "+X points" notification
- [ ] Show confetti animation (optional)
- [ ] Update lesson completion badge
- [ ] Prevent multiple award triggers

#### Premium Content
- [ ] Check user subscription status
- [ ] Create PremiumVideoGate component
- [ ] Show upgrade prompt for premium videos
- [ ] Display video preview/thumbnail
- [ ] Add "Upgrade to Premium" button
- [ ] Link to pricing page

#### Video Controls
- [ ] Playback speed selector (0.5x, 1x, 1.5x, 2x)
- [ ] Quality selector (if available)
- [ ] Fullscreen toggle
- [ ] Volume control (use YouTube's default)
- [ ] Progress bar with hover preview
- [ ] Current time / duration display

#### Error Handling
- [ ] Handle video not found (404)
- [ ] Handle video unavailable (private/deleted)
- [ ] Handle network errors
- [ ] Handle API errors
- [ ] Display user-friendly error messages
- [ ] Provide recovery options
- [ ] Log errors for debugging

#### Progress Visualization
- [ ] Create ProgressBar component
- [ ] Show completion percentage
- [ ] Highlight completed portions
- [ ] Show current position indicator
- [ ] Add hover effect with timestamp
- [ ] Animate progress updates

#### Testing
- [ ] Unit tests for ProgressService
- [ ] Integration tests for progress endpoints
- [ ] Component tests for LessonPlayer
- [ ] Test resume functionality
- [ ] Test completion detection
- [ ] Test point awards
- [ ] Test error scenarios
- [ ] Test on various devices/browsers

#### Documentation
- [ ] Document progress tracking algorithm
- [ ] Add code comments for complex logic
- [ ] Create user guide for video player
- [ ] Document keyboard shortcuts

---

## /speckit.tasks

### Task Breakdown (Estimated 25 hours)

#### Task 1: Progress Service Implementation (4 hours)
**Description:** Create backend service for progress tracking
**Subtasks:**
1. Review UserProgress entity, add TotalWatchTime if missing
2. Create IProgressService interface with all methods
3. Implement GetOrCreateProgressAsync
4. Implement UpdateProgressAsync with completion logic
5. Implement GetProgressAsync
6. Add unit tests for ProgressService
7. Test completion detection at 80% threshold
8. Test point award integration

#### Task 2: Progress API Endpoints (3 hours)
**Description:** Create REST endpoints for progress operations
**Subtasks:**
1. Create ProgressController
2. Implement GET /api/lessons/{id}/progress
3. Implement PUT /api/lessons/{id}/progress with validation
4. Implement POST /api/lessons/{id}/complete
5. Create DTOs (ProgressDto, UpdateProgressDto)
6. Add authorization and rate limiting
7. Test endpoints with Postman
8. Add Swagger documentation

#### Task 3: Video Player Component (5 hours)
**Description:** Build main video player with react-player
**Subtasks:**
1. Install react-player library
2. Create LessonPlayer component structure
3. Set up react-player with YouTube config
4. Add player state management
5. Implement play/pause controls
6. Add keyboard shortcuts
7. Style player with TailwindCSS
8. Make responsive for mobile
9. Test on various devices

#### Task 4: Progress Tracking Hook (4 hours)
**Description:** Create custom hook for automatic progress tracking
**Subtasks:**
1. Create useVideoProgress custom hook
2. Implement progress load on mount
3. Add auto-save interval (every 10 seconds)
4. Implement debounced API calls
5. Handle save on pause and unmount
6. Add error handling and retry logic
7. Test progress persistence
8. Optimize for performance

#### Task 5: Resume Functionality (3 hours)
**Description:** Implement resume from last position
**Subtasks:**
1. Fetch saved progress on component mount
2. Create resume prompt UI (if position > 30s)
3. Implement seek to saved position
4. Add "Start from beginning" option
5. Handle cases where no saved progress exists
6. Test resume across page refreshes

#### Task 6: Completion Detection & Points (3 hours)
**Description:** Implement completion detection and point rewards
**Subtasks:**
1. Track completion percentage in real-time
2. Detect 80% threshold crossing
3. Call complete endpoint
4. Display point award notification
5. Add confetti animation (optional)
6. Update lesson completion status in UI
7. Test point award only triggers once
8. Handle errors in point award

#### Task 7: Premium Content Gates (2 hours)
**Description:** Implement access control for premium videos
**Subtasks:**
1. Check user subscription before loading video
2. Create PremiumVideoGate component
3. Design upgrade prompt UI
4. Add preview/thumbnail display
5. Link to pricing page
6. Test with free and premium accounts

#### Task 8: Error Handling (2 hours)
**Description:** Comprehensive error handling
**Subtasks:**
1. Handle unavailable videos (404, private, deleted)
2. Handle network errors
3. Handle API errors
4. Create user-friendly error messages
5. Add recovery options
6. Test all error scenarios

#### Task 9: Video Controls & UX (3 hours)
**Description:** Enhanced video controls and user experience
**Subtasks:**
1. Add playback speed selector
2. Implement quality selector
3. Create progress bar with hover preview
4. Add current time / duration display
5. Improve loading states
6. Polish UI/UX
7. Test user interactions

#### Task 10: Testing & Optimization (3 hours)
**Description:** Comprehensive testing and performance optimization
**Subtasks:**
1. Write unit tests for ProgressService
2. Write integration tests for API endpoints
3. Write component tests for LessonPlayer
4. Test on multiple browsers
5. Test on mobile devices
6. Optimize bundle size
7. Optimize API calls (debouncing, caching)
8. Fix any bugs found

---

## /speckit.implement

### Implementation Code

#### ProgressService

**File:** `WahadiniCryptoQuest.Application/Services/ProgressService.cs`

```csharp
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;
using WahadiniCryptoQuest.Domain.Entities;
using WahadiniCryptoQuest.Domain.Enums;

namespace WahadiniCryptoQuest.Application.Services;

public class ProgressService : IProgressService
{
    private readonly IProgressRepository _progressRepository;
    private readonly ILessonRepository _lessonRepository;
    private readonly IRewardService _rewardService;
    private readonly ILogger<ProgressService> _logger;
    
    public ProgressService(
        IProgressRepository progressRepository,
        ILessonRepository lessonRepository,
        IRewardService rewardService,
        ILogger<ProgressService> logger)
    {
        _progressRepository = progressRepository;
        _lessonRepository = lessonRepository;
        _rewardService = rewardService;
        _logger = logger;
    }
    
    public async Task<ProgressDto?> GetProgressAsync(Guid userId, Guid lessonId)
    {
        var progress = await _progressRepository.GetByUserAndLessonAsync(userId, lessonId);
        
        if (progress == null)
        {
            return null;
        }
        
        return new ProgressDto
        {
            LessonId = progress.LessonId,
            LastWatchedPosition = progress.LastWatchedPosition,
            CompletionPercentage = progress.CompletionPercentage,
            IsCompleted = progress.IsCompleted,
            CompletedAt = progress.CompletedAt,
            TotalWatchTime = progress.TotalWatchTime ?? 0
        };
    }
    
    public async Task<UpdateProgressResultDto> UpdateProgressAsync(
        Guid userId, Guid lessonId, int watchPosition)
    {
        var progress = await GetOrCreateProgressAsync(userId, lessonId);
        var lesson = await _lessonRepository.GetByIdAsync(lessonId);
        
        if (lesson == null)
        {
            throw new NotFoundException("Lesson not found");
        }
        
        // Validate watch position
        if (watchPosition < 0 || watchPosition > lesson.Duration * 60) // Duration in minutes, convert to seconds
        {
            throw new ValidationException("Invalid watch position");
        }
        
        // Update position (only if it's ahead of current position)
        if (watchPosition > progress.LastWatchedPosition)
        {
            progress.LastWatchedPosition = watchPosition;
        }
        
        // Update total watch time
        if (!progress.TotalWatchTime.HasValue)
        {
            progress.TotalWatchTime = 10;
        }
        else
        {
            progress.TotalWatchTime += 10;
        }
        
        // Calculate completion percentage
        var totalDurationSeconds = lesson.Duration * 60;
        var completionPercentage = (watchPosition / (double)totalDurationSeconds) * 100;
        progress.CompletionPercentage = (decimal)Math.Min(completionPercentage, 100);
        
        progress.LastUpdatedAt = DateTime.UtcNow;
        
        // Check for completion (80% threshold)
        bool isNewlyCompleted = false;
        int pointsAwarded = 0;
        
        if (completionPercentage >= 80 && !progress.IsCompleted)
        {
            progress.IsCompleted = true;
            progress.CompletedAt = DateTime.UtcNow;
            
            // Award points (only if not already claimed)
            if (!progress.RewardPointsClaimed)
            {
                try
                {
                    await _rewardService.AwardPointsAsync(
                        userId,
                        lesson.RewardPoints,
                        TransactionType.Earned,
                        lessonId.ToString(),
                        $"Completed lesson: {lesson.Title}"
                    );
                    
                    progress.RewardPointsClaimed = true;
                    pointsAwarded = lesson.RewardPoints;
                    isNewlyCompleted = true;
                    
                    _logger.LogInformation(
                        "User {UserId} completed lesson {LessonId}, awarded {Points} points",
                        userId, lessonId, pointsAwarded);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Failed to award points for lesson {LessonId}", lessonId);
                    // Don't throw - progress is still saved
                }
            }
        }
        
        await _progressRepository.UpdateAsync(progress);
        
        return new UpdateProgressResultDto
        {
            Success = true,
            CompletionPercentage = progress.CompletionPercentage,
            PointsAwarded = pointsAwarded,
            IsNewlyCompleted = isNewlyCompleted
        };
    }
    
    private async Task<UserProgress> GetOrCreateProgressAsync(Guid userId, Guid lessonId)
    {
        var progress = await _progressRepository.GetByUserAndLessonAsync(userId, lessonId);
        
        if (progress == null)
        {
            progress = new UserProgress
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                LessonId = lessonId,
                LastWatchedPosition = 0,
                CompletionPercentage = 0,
                IsCompleted = false,
                RewardPointsClaimed = false,
                LastUpdatedAt = DateTime.UtcNow,
                TotalWatchTime = 0
            };
            
            await _progressRepository.AddAsync(progress);
        }
        
        return progress;
    }
}
```

#### ProgressController

**File:** `WahadiniCryptoQuest.API/Controllers/ProgressController.cs`

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using WahadiniCryptoQuest.Application.DTOs;
using WahadiniCryptoQuest.Application.Interfaces;

namespace WahadiniCryptoQuest.API.Controllers;

[ApiController]
[Route("api/lessons")]
[Authorize]
public class ProgressController : ControllerBase
{
    private readonly IProgressService _progressService;
    private readonly ILogger<ProgressController> _logger;
    
    public ProgressController(
        IProgressService progressService,
        ILogger<ProgressController> logger)
    {
        _progressService = progressService;
        _logger = logger;
    }
    
    /// <summary>
    /// Get user's progress for a specific lesson
    /// </summary>
    [HttpGet("{lessonId}/progress")]
    public async Task<IActionResult> GetProgress(Guid lessonId)
    {
        var userId = GetCurrentUserId();
        var progress = await _progressService.GetProgressAsync(userId, lessonId);
        
        if (progress == null)
        {
            return Ok(new ProgressDto
            {
                LessonId = lessonId,
                LastWatchedPosition = 0,
                CompletionPercentage = 0,
                IsCompleted = false,
                TotalWatchTime = 0
            });
        }
        
        return Ok(progress);
    }
    
    /// <summary>
    /// Update user's watch progress for a lesson
    /// </summary>
    [HttpPut("{lessonId}/progress")]
    [EnableRateLimiting("progress-update")] // Max 1 per 5 seconds
    public async Task<IActionResult> UpdateProgress(
        Guid lessonId,
        [FromBody] UpdateProgressDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }
        
        try
        {
            var userId = GetCurrentUserId();
            var result = await _progressService.UpdateProgressAsync(
                userId, lessonId, dto.WatchPosition);
            
            return Ok(result);
        }
        catch (NotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (ValidationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating progress for lesson {LessonId}", lessonId);
            return StatusCode(500, new { message = "An error occurred while updating progress" });
        }
    }
    
    private Guid GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        return Guid.Parse(userIdClaim!);
    }
}
```

#### Frontend LessonPlayer Component

**File:** `frontend/src/components/lesson/LessonPlayer.tsx`

```typescript
import React, { useState, useRef, useEffect } from 'react';
import ReactPlayer from 'react-player/youtube';
import { Play, Pause, Volume2, Maximize, Loader2 } from 'lucide-react';
import { useVideoProgress } from '../../hooks/useVideoProgress';
import { Lesson } from '../../types/course.types';
import toast from 'react-hot-toast';

interface LessonPlayerProps {
  lesson: Lesson;
  userId: string;
}

export const LessonPlayer: React.FC<LessonPlayerProps> = ({ lesson, userId }) => {
  const playerRef = useRef<ReactPlayer>(null);
  const [playing, setPlaying] = useState(false);
  const [playbackRate, setPlaybackRate] = useState(1);
  const [showResumePrompt, setShowResumePrompt] = useState(false);
  
  const {
    progress,
    loading,
    updateProgress,
    completeLesson
  } = useVideoProgress(lesson.id, userId);
  
  const saveIntervalRef = useRef<NodeJS.Timeout | null>(null);
  
  // Load saved progress on mount
  useEffect(() => {
    if (progress && progress.lastWatchedPosition > 30 && !progress.isCompleted) {
      setShowResumePrompt(true);
    }
  }, [progress]);
  
  // Auto-save progress every 10 seconds while playing
  useEffect(() => {
    if (playing) {
      saveIntervalRef.current = setInterval(() => {
        saveProgress();
      }, 10000);
    } else {
      if (saveIntervalRef.current) {
        clearInterval(saveIntervalRef.current);
      }
    }
    
    return () => {
      if (saveIntervalRef.current) {
        clearInterval(saveIntervalRef.current);
      }
    };
  }, [playing]);
  
  // Save progress on unmount
  useEffect(() => {
    return () => {
      saveProgress();
    };
  }, []);
  
  const saveProgress = async () => {
    if (!playerRef.current) return;
    
    const currentTime = playerRef.current.getCurrentTime();
    if (!currentTime || currentTime < 1) return;
    
    try {
      const result = await updateProgress(Math.floor(currentTime));
      
      // If newly completed, show notification
      if (result.isNewlyCompleted && result.pointsAwarded > 0) {
        toast.success(`Lesson completed! +${result.pointsAwarded} points 🎉`, {
          duration: 5000,
          icon: '🏆',
        });
      }
    } catch (error) {
      console.error('Failed to save progress:', error);
    }
  };
  
  const handleProgress = (state: { playedSeconds: number; played: number }) => {
    // Progress is tracked via auto-save interval
  };
  
  const handleEnded = () => {
    setPlaying(false);
    saveProgress();
  };
  
  const handleResume = () => {
    if (progress && playerRef.current) {
      playerRef.current.seekTo(progress.lastWatchedPosition, 'seconds');
      setShowResumePrompt(false);
      setPlaying(true);
    }
  };
  
  const handleStartFromBeginning = () => {
    setShowResumePrompt(false);
    setPlaying(true);
  };
  
  const handlePlayPause = () => {
    if (!playing) {
      setPlaying(true);
    } else {
      setPlaying(false);
      saveProgress();
    }
  };
  
  // Keyboard shortcuts
  useEffect(() => {
    const handleKeyPress = (e: KeyboardEvent) => {
      if (e.target instanceof HTMLInputElement || e.target instanceof HTMLTextAreaElement) {
        return;
      }
      
      switch (e.key) {
        case ' ':
          e.preventDefault();
          handlePlayPause();
          break;
        case 'f':
        case 'F':
          e.preventDefault();
          // Trigger fullscreen
          break;
        case 'ArrowLeft':
          e.preventDefault();
          if (playerRef.current) {
            const current = playerRef.current.getCurrentTime();
            playerRef.current.seekTo(Math.max(0, current - 10), 'seconds');
          }
          break;
        case 'ArrowRight':
          e.preventDefault();
          if (playerRef.current) {
            const current = playerRef.current.getCurrentTime();
            playerRef.current.seekTo(current + 10, 'seconds');
          }
          break;
      }
    };
    
    window.addEventListener('keydown', handleKeyPress);
    return () => window.removeEventListener('keydown', handleKeyPress);
  }, [playing]);
  
  if (loading) {
    return (
      <div className="aspect-video bg-gray-900 rounded-lg flex items-center justify-center">
        <Loader2 className="w-12 h-12 animate-spin text-white" />
      </div>
    );
  }
  
  return (
    <div className="relative">
      <div className="aspect-video bg-black rounded-lg overflow-hidden">
        <ReactPlayer
          ref={playerRef}
          url={`https://www.youtube.com/watch?v=${lesson.youtubeVideoId}`}
          width="100%"
          height="100%"
          playing={playing}
          playbackRate={playbackRate}
          controls
          onPlay={() => setPlaying(true)}
          onPause={() => {
            setPlaying(false);
            saveProgress();
          }}
          onProgress={handleProgress}
          onEnded={handleEnded}
          config={{
            youtube: {
              playerVars: {
                modestbranding: 1,
                rel: 0,
                fs: 1,
              },
            },
          }}
        />
        
        {/* Progress indicator */}
        {progress && (
          <div className="absolute bottom-0 left-0 right-0 h-1 bg-gray-700">
            <div
              className="h-full bg-green-500 transition-all duration-300"
              style={{ width: `${progress.completionPercentage}%` }}
            />
          </div>
        )}
        
        {/* Completion badge */}
        {progress?.isCompleted && (
          <div className="absolute top-4 right-4 bg-green-500 text-white px-3 py-1 rounded-full text-sm font-semibold flex items-center gap-2">
            ✓ Completed
          </div>
        )}
      </div>
      
      {/* Resume prompt */}
      {showResumePrompt && progress && (
        <div className="mt-4 p-4 bg-blue-50 dark:bg-blue-900 rounded-lg">
          <p className="text-sm text-blue-800 dark:text-blue-200 mb-3">
            Resume from {formatTime(progress.lastWatchedPosition)}?
          </p>
          <div className="flex gap-2">
            <button
              onClick={handleResume}
              className="px-4 py-2 bg-blue-600 text-white rounded-md hover:bg-blue-700 transition"
            >
              Resume
            </button>
            <button
              onClick={handleStartFromBeginning}
              className="px-4 py-2 bg-gray-200 text-gray-800 rounded-md hover:bg-gray-300 transition"
            >
              Start from beginning
            </button>
          </div>
        </div>
      )}
      
      {/* Playback speed controls */}
      <div className="mt-4 flex items-center gap-4">
        <label className="text-sm text-gray-600 dark:text-gray-400">
          Speed:
        </label>
        {[0.5, 1, 1.5, 2].map((rate) => (
          <button
            key={rate}
            onClick={() => setPlaybackRate(rate)}
            className={`px-3 py-1 rounded text-sm transition ${
              playbackRate === rate
                ? 'bg-primary-600 text-white'
                : 'bg-gray-200 text-gray-700 hover:bg-gray-300'
            }`}
          >
            {rate}x
          </button>
        ))}
      </div>
    </div>
  );
};

function formatTime(seconds: number): string {
  const mins = Math.floor(seconds / 60);
  const secs = seconds % 60;
  return `${mins}:${secs.toString().padStart(2, '0')}`;
}

export default LessonPlayer;
```

#### useVideoProgress Hook

**File:** `frontend/src/hooks/useVideoProgress.ts`

```typescript
import { useState, useEffect } from 'react';
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query';
import { progressService } from '../services/progressService';
import { ProgressDto, UpdateProgressResultDto } from '../types/progress.types';

export const useVideoProgress = (lessonId: string, userId: string) => {
  const queryClient = useQueryClient();
  
  // Fetch progress
  const { data: progress, isLoading } = useQuery({
    queryKey: ['progress', lessonId, userId],
    queryFn: () => progressService.getProgress(lessonId),
  });
  
  // Update progress mutation
  const updateMutation = useMutation({
    mutationFn: (watchPosition: number) =>
      progressService.updateProgress(lessonId, watchPosition),
    onSuccess: (data) => {
      queryClient.setQueryData(['progress', lessonId, userId], (old: ProgressDto) => ({
        ...old,
        lastWatchedPosition: data.watchPosition,
        completionPercentage: data.completionPercentage,
        isCompleted: data.isNewlyCompleted || old?.isCompleted,
      }));
    },
  });
  
  const updateProgress = async (watchPosition: number): Promise<UpdateProgressResultDto> => {
    const result = await updateMutation.mutateAsync(watchPosition);
    return result;
  };
  
  const completeLesson = async (): Promise<number> => {
    // Trigger completion
    const result = await updateProgress(999999); // High number to ensure completion
    return result.pointsAwarded;
  };
  
  return {
    progress,
    loading: isLoading,
    updateProgress,
    completeLesson,
  };
};
```

### Notes
- Implement rate limiting on progress update endpoint (1 per 5 seconds)
- Add retry logic for failed progress saves
- Consider adding analytics tracking for watch time
- Test extensively on mobile devices
- Monitor API call frequency to optimize
- Consider WebSocket for real-time progress updates in future
