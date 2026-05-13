# Feature Specification: YouTube Video Player with Progress Tracking

**Feature Branch**: `004-youtube-video-player`  
**Created**: 2025-11-16  
**Status**: Draft  
**Input**: User description: "Implement a robust YouTube video player with automatic progress tracking, resume functionality, completion detection, and point rewards. This is a core feature that enables users to watch educational content and track their learning progress."

## Clarifications

### Session 2025-11-16

- Q: What happens if a user skips ahead in the video? → A: Track the highest watched position. Skipping forward updates progress. Future enhancement may add fraud detection if needed.
- Q: How do we handle users who reload the page mid-video? → A: Progress is saved every 10 seconds to backend. On reload, fetch last position and offer to resume.
- Q: What if YouTube video is deleted or made private? → A: Display error message "Video unavailable". Admin can update video ID. User progress is preserved.
- Q: Should we prevent users from earning points by skipping to the end? → A: For MVP, allow it. Future enhancement: require watching at least 80% of video duration (not just seeking to 80%).
- Q: How precise should progress tracking be? → A: Save position every 10 seconds. This balances accuracy with API call frequency.
- Q: What if user watches on multiple devices? → A: Use last updated position. If user continues on device B after watching on device A, they can resume from device A's position.
- Q: Should we show a "Mark as complete" button? → A: No, completion is automatic at 80%. This ensures users actually watch content.
- Q: How do we handle very short videos (< 2 minutes)? → A: Completion threshold applies: 80% of any duration. A 1-minute video needs 48 seconds watched.
- Q: Can users replay videos after completion? → A: Yes, they can rewatch anytime. Progress updates but points are only awarded once.
- Q: What about network interruptions during save? → A: Implement retry logic with exponential backoff. Queue failed saves in localStorage for later sync.
- Q: Should we track watch time for analytics? → A: Yes, store total watch time in UserProgress for analytics purposes.
- Q: How to handle concurrent viewing sessions? → A: Last write wins. If user watches same video on two devices simultaneously, latest progress update is saved.

## User Scenarios & Testing *(mandatory)*

### User Story 1 - Basic Video Playback (Priority: P1)

A user opens a lesson and watches the YouTube video embedded in the lesson player. The video plays smoothly with standard YouTube controls and responds to user interactions.

**Why this priority**: This is the foundation of the learning experience. Without working video playback, no other features matter. It delivers immediate value by allowing users to access educational content.

**Independent Test**: Can be fully tested by navigating to any lesson page with a YouTube video URL and verifying the video loads and plays correctly. Delivers the core value of content delivery without requiring other features.

**Acceptance Scenarios**:

1. **Given** a user is on a lesson page with a valid YouTube video URL, **When** the page loads, **Then** the video player renders with the YouTube video ready to play
2. **Given** the video is loaded, **When** the user clicks the play button, **Then** the video starts playing with audio
3. **Given** the video is playing, **When** the user clicks pause, **Then** the video pauses at the current timestamp
4. **Given** the video player is displayed, **When** the user drags the progress bar, **Then** the video seeks to the selected timestamp
5. **Given** a lesson has an invalid or deleted YouTube video, **When** the page loads, **Then** the user sees a clear error message indicating the video is unavailable

---

### User Story 2 - Automatic Progress Tracking (Priority: P1)

As a user watches a video, the system automatically saves their current watch position every 10 seconds. If the user closes the browser or navigates away, their progress is preserved without any manual action.

**Why this priority**: Users expect modern video platforms to remember where they left off. This is essential for user retention and preventing frustration from losing progress on long videos.

**Independent Test**: Can be tested by watching a video for 30 seconds, refreshing the page, and verifying the video resumes near the last position. Delivers standalone value of progress persistence.

**Acceptance Scenarios**:

1. **Given** a user is watching a video, **When** 10 seconds have elapsed since the last save, **Then** the system saves the current watch position to the database
2. **Given** a user has watched 2 minutes of a 10-minute video, **When** they close the browser tab, **Then** the watch position (approximately 2 minutes) is saved
3. **Given** progress tracking is active, **When** saving progress fails due to network error, **Then** the system retries the save operation without disrupting video playback
4. **Given** a user is watching a video, **When** they seek to different timestamps multiple times, **Then** only the most recent position is saved to reduce database writes

---

### User Story 3 - Resume from Last Position (Priority: P1)

When a user returns to a lesson they've partially watched, the video automatically resumes from where they left off instead of starting from the beginning.

**Why this priority**: This completes the progress tracking user experience loop. Without resume functionality, saving progress provides no value. Together they form the minimum viable progress tracking system.

**Independent Test**: Can be tested by watching part of a video, navigating away, returning to the same lesson, and verifying the video offers to resume or automatically resumes from the saved position.

**Acceptance Scenarios**:

1. **Given** a user has previously watched 3 minutes of a video, **When** they open the same lesson again, **Then** the video player displays a "Resume from 3:00" prompt or automatically starts from that position
2. **Given** a user has completed a video (watched 100%), **When** they reopen the lesson, **Then** the video starts from the beginning (since it's already complete)
3. **Given** a user has watched less than 5 seconds of a video, **When** they return to the lesson, **Then** the video starts from the beginning (minimal progress threshold)
4. **Given** the saved progress timestamp exceeds the video duration, **When** loading the video, **Then** the video starts from the beginning (handles video duration changes)

---

### User Story 4 - Completion Detection and Point Rewards (Priority: P2)

When a user watches at least 80% of a video, the system automatically marks it as complete and awards points. This happens once per video, and users receive immediate feedback about their achievement.

**Why this priority**: This gamification element motivates users and provides tangible progress indicators. While important for engagement, videos can still be watched and learned from without the points system.

**Independent Test**: Can be tested by watching a video past the 80% threshold and verifying the completion status updates and points are awarded. Works independently with just the video player and rewards system.

**Acceptance Scenarios**:

1. **Given** a user is watching a video and reaches the 80% completion mark, **When** the completion threshold is crossed, **Then** the system marks the video as complete and awards the designated points
2. **Given** a user has already completed a video and earned points, **When** they watch the same video again, **Then** no additional points are awarded (prevents point farming)
3. **Given** a video is marked complete, **When** the completion is detected, **Then** the user sees a visual notification (toast/popup) confirming completion and points earned
4. **Given** a user skips directly to 90% of the video without watching earlier content, **When** the completion check runs, **Then** the video is marked complete (allows users to skip known content)
5. **Given** the point award operation fails, **When** completion is detected, **Then** the video is still marked complete and the point award is retried in the background

---

### User Story 5 - Keyboard Navigation and Playback Controls (Priority: P2)

Users can control video playback using keyboard shortcuts (Space for play/pause, F for fullscreen, arrow keys for seeking) and adjust playback speed (0.5x, 1x, 1.5x, 2x).

**Why this priority**: Power users and accessibility needs benefit greatly from keyboard controls and speed adjustments. These enhance the experience but aren't required for basic video watching.

**Independent Test**: Can be tested by using keyboard shortcuts during video playback and verifying the player responds appropriately. Speed controls can be tested by selecting different speeds and confirming audio/video adjustment.

**Acceptance Scenarios**:

1. **Given** a video is playing and the player has focus, **When** the user presses Space, **Then** the video pauses
2. **Given** a video is paused and the player has focus, **When** the user presses Space, **Then** the video resumes playing
3. **Given** a video is playing, **When** the user presses the F key, **Then** the video enters fullscreen mode
4. **Given** a video is playing, **When** the user presses the right arrow key, **Then** the video seeks forward by 10 seconds
5. **Given** a video is playing, **When** the user presses the left arrow key, **Then** the video seeks backward by 10 seconds
6. **Given** the playback speed control is visible, **When** the user selects 1.5x speed, **Then** the video and audio play at 1.5 times normal speed
7. **Given** the user changes playback speed, **When** the speed is set, **Then** the selection is saved and persists for future videos in the same session

---

### User Story 6 - Premium Content Access Control (Priority: P2)

Free users can see premium lesson titles and descriptions but cannot watch premium videos. Premium/paid users have full access to all videos. The system clearly indicates which content requires a subscription.

**Why this priority**: This is essential for the business model but doesn't affect the core learning experience for free users accessing free content. It can be implemented after basic playback works.

**Independent Test**: Can be tested by accessing premium content as a free user (should see upgrade prompt), then as a premium user (should play normally). Works independently with authentication and subscription status.

**Acceptance Scenarios**:

1. **Given** a free user views a premium lesson page, **When** the page loads, **Then** they see the lesson description but the video player shows an upgrade prompt instead of the video
2. **Given** a premium user views a premium lesson page, **When** the page loads, **Then** the video loads and plays normally
3. **Given** a free user is on a premium lesson, **When** they click the upgrade prompt, **Then** they are directed to the subscription/payment page
4. **Given** a free user views a free lesson, **When** the page loads, **Then** the video plays normally without any restrictions
5. **Given** a user's subscription expires while watching a premium video, **When** they try to play another premium video, **Then** they see the upgrade prompt

---

### User Story 7 - Mobile Responsive Player (Priority: P3)

The video player adapts to different screen sizes (desktop, tablet, mobile) with appropriate controls and layout adjustments. Touch controls work smoothly on mobile devices.

**Why this priority**: Mobile experience is important for modern learners but the core functionality can be validated on desktop first. This enhances accessibility without blocking the core feature.

**Independent Test**: Can be tested by opening lessons on different device sizes and verifying the player scales appropriately and remains functional. Works independently once the desktop player is complete.

**Acceptance Scenarios**:

1. **Given** a user opens a lesson on a mobile device (< 768px width), **When** the page loads, **Then** the video player scales to fit the screen width while maintaining aspect ratio
2. **Given** a user is watching on a tablet (768px - 1024px width), **When** they interact with controls, **Then** touch targets are appropriately sized (minimum 44x44px)
3. **Given** a user rotates their mobile device, **When** the orientation changes, **Then** the player adjusts its layout without losing playback position
4. **Given** a user is on mobile, **When** they tap the video, **Then** the play/pause toggle responds immediately

---

### User Story 8 - Progress Visualization (Priority: P3)

Users can see a visual indicator of their progress through each video, both on the lesson page and in course overviews. This helps them track their learning journey.

**Why this priority**: While helpful for user motivation, this is a visual enhancement that doesn't affect the core video watching or progress tracking functionality.

**Independent Test**: Can be tested by watching portions of videos and verifying progress bars/percentages update correctly on both the player and course overview pages.

**Acceptance Scenarios**:

1. **Given** a user is on a lesson page, **When** the video player loads, **Then** a progress bar shows their current watch percentage (e.g., "35% complete")
2. **Given** a user views a course overview, **When** the page displays lessons, **Then** each lesson shows a visual progress indicator (e.g., progress bar or percentage)
3. **Given** a user has completed a video, **When** viewing the lesson or course, **Then** the video shows a "Completed" badge or 100% indicator
4. **Given** a user has not started a video, **When** viewing the lesson, **Then** the progress shows 0% or "Not started"

---

### Edge Cases

- What happens when a YouTube video is deleted or set to private after being added to a lesson?
  - System should detect the error and display a clear message to the user
  - Admin should be notified (via dashboard or email) that the video needs updating
  - Progress tracking should not break or throw errors

- How does the system handle extremely long videos (2+ hours)?
  - Progress saves every 10 seconds remain consistent
  - Completion threshold of 80% still applies (user must watch 1.6+ hours)
  - Video player should not have performance issues with long durations

- What happens when a user watches a video on multiple devices simultaneously?
  - System uses the most recent progress update from any device
  - No data loss; race conditions handled by timestamp comparison
  - User sees consistent resume point when switching devices

- How does the system handle rapid seeking (user quickly skipping through video)?
  - Track highest position reached (forward seeks update progress, backward seeks do not)
  - Debounce progress saves to avoid excessive database writes
  - Only save when seeking stops or at 10-second intervals
  - Completion detection works correctly (80% threshold based on highest position)
  - For MVP, users can skip to end and earn points; future enhancement may require actual watch time verification

- What happens if the video duration changes (YouTube video is re-uploaded or edited)?
  - System compares saved timestamp to current duration
  - If saved timestamp > current duration, start from beginning
  - Progress percentage recalculates based on new duration

- How does the system handle network interruptions during video playback?
  - Video buffering handled by YouTube player (browser native behavior)
  - Progress save failures retry automatically with exponential backoff (1s, 2s, 4s, 8s intervals)
  - Failed saves queued in localStorage and synchronized when network returns
  - User can continue watching; progress tracking resumes automatically when connection restored

- What happens when a user watches without being logged in (guest)?
  - Free content plays normally without tracking (if allowed by business rules)
  - Premium content shows upgrade/login prompt
  - Option to prompt guest to create account to save progress

## Requirements *(mandatory)*

### Functional Requirements

- **FR-001**: System MUST embed YouTube videos using the react-player library with the YouTube video ID extracted from lesson data
- **FR-002**: System MUST automatically save the current watch position (highest position reached) to the database every 10 seconds during video playback, tracking forward seeks but not backward seeks
- **FR-003**: System MUST load the user's last saved watch position when opening a previously watched lesson and either auto-resume or prompt to resume (if position > 5 seconds)
- **FR-004**: System MUST detect when a user reaches 80% completion of a video and mark it as complete in the database
- **FR-005**: System MUST award the designated point value to the user's account when a video is marked complete for the first time
- **FR-006**: System MUST prevent duplicate point awards for the same video completion by checking completion status before awarding points
- **FR-007**: System MUST support keyboard shortcuts: Space (play/pause), F (fullscreen), Left Arrow (seek -10s), Right Arrow (seek +10s)
- **FR-008**: System MUST provide playback speed controls with options for 0.5x, 0.75x, 1x, 1.25x, 1.5x, and 2x speeds
- **FR-009**: System MUST display a clear error message when a YouTube video is unavailable, deleted, or private
- **FR-010**: System MUST prevent free users from playing premium video content and display an upgrade prompt instead
- **FR-011**: System MUST allow premium/paid users to access all premium video content without restrictions
- **FR-012**: System MUST render video players responsively on desktop (1024px+), tablet (768-1024px), and mobile (< 768px) screen sizes
- **FR-013**: System MUST display visual progress indicators showing watch percentage for each lesson
- **FR-014**: System MUST debounce progress save operations to avoid excessive database writes during seeking or rapid interactions
- **FR-015**: System MUST handle race conditions when a user watches the same video on multiple devices by using the most recent timestamp (last write wins)
- **FR-016**: System MUST show a visual notification (toast/modal) when a user completes a video and earns points
- **FR-021**: System MUST implement retry logic with exponential backoff for failed progress save operations and queue failed saves in localStorage for later synchronization
- **FR-022**: System MUST track total watch time in UserProgress entity for analytics purposes, incrementing by the save interval (10 seconds) on each successful save
- **FR-017**: System MUST persist playback speed preference across videos within the same user session
- **FR-018**: System MUST validate YouTube video URLs and extract video IDs correctly from various YouTube URL formats
- **FR-019**: System MUST allow users to manually restart completed videos from the beginning if desired
- **FR-020**: System MUST maintain video aspect ratio (16:9 typical) across all screen sizes

### Constitution Compliance Requirements

**Learning-First**: The video player prioritizes uninterrupted learning experience. Progress tracking operates silently without disrupting playback. Completion detection focuses on meaningful engagement (80% threshold) rather than encouraging users to game the system. Point rewards motivate completion but don't distract from content quality.

**Security & Privacy**: All progress data is associated with authenticated users via JWT tokens. User watch history and progress data is private and not shared. API endpoints for progress updates require authentication. Input validation prevents malicious YouTube URLs or XSS attacks via video metadata.

**Scalability**: Progress tracking uses debounced saves (10-second intervals) to minimize database writes. Video playback relies on YouTube's CDN infrastructure for scalability. Progress data uses indexed database queries for fast retrieval. System supports 1000+ concurrent users watching videos with < 500ms API response times for progress operations.

**Fair Economy**: Point awards occur once per video completion to prevent farming. The 80% completion threshold requires meaningful engagement. Users cannot earn unlimited points by repeatedly watching the same video. Point transactions are logged for audit trails and fraud detection.

**Quality Assurance**: Videos must be validated before being assigned to lessons (admin verification). Broken or unavailable videos are clearly indicated. System gracefully handles YouTube API changes or video deletions. Error messages provide clear guidance without exposing technical details.

**Accessibility**: Keyboard shortcuts enable navigation without a mouse. Screen readers can access player controls. Video captions/subtitles are supported through YouTube's native features. Color contrast meets WCAG 2.1 AA standards for player controls. Touch targets on mobile are minimum 44x44px.

**Business Ethics**: Free users can access free content without restrictions. Premium content gates are transparent with clear upgrade paths. No misleading "Play" buttons on premium content for free users. User progress data is not sold to third parties. Users can delete their watch history.

**Technical Excellence**: Clean Architecture separation: Domain entities (UserProgress, LessonCompletion), Application services (ProgressTrackingService), Infrastructure (YouTube API integration). Comprehensive error handling prevents crashes. Test coverage includes unit tests for progress logic and integration tests for API endpoints. CI/CD pipeline validates video player functionality.

### Key Entities

- **UserProgress**: Represents a user's progress through a specific lesson video
  - Attributes: UserId, LessonId, LastWatchPosition (seconds), WatchPercentage, LastUpdated, CompletedAt, TotalWatchTime (seconds)
  - Relationships: Belongs to User, Belongs to Lesson
  - Business rules: Only one progress record per User-Lesson pair; LastWatchPosition must be >= 0 and <= video duration; tracks highest position reached for forward seeks

- **LessonCompletion**: Represents the completion status of a lesson by a user
  - Attributes: UserId, LessonId, CompletedAt, PointsAwarded, CompletionPercentage
  - Relationships: Belongs to User, Belongs to Lesson
  - Business rules: Completion triggered at 80% watch threshold; points awarded only once per User-Lesson pair

- **Lesson**: Enhanced with video-related attributes
  - Attributes: Id, CourseId, Title, Description, YouTubeVideoUrl, VideoDuration, PointValue, IsPremium, Order
  - Relationships: Belongs to Course, Has many UserProgress records, Has many LessonCompletions
  - Business rules: YouTubeVideoUrl must be valid YouTube URL; VideoDuration updated when video is loaded; PointValue must be > 0

- **RewardTransaction**: Records point awards for video completions
  - Attributes: Id, UserId, LessonId, PointsAwarded, TransactionType, CreatedAt, Reason
  - Relationships: Belongs to User, Belongs to Lesson
  - Business rules: TransactionType = "LessonCompletion"; immutable once created; used for audit trail

## Success Criteria *(mandatory)*

### Measurable Outcomes

- **SC-001**: Users can watch lesson videos without interruption, with video loading and playback starting within 3 seconds on average
- **SC-002**: Users' watch progress is automatically saved every 10 seconds with 99%+ reliability
- **SC-003**: When returning to a partially watched lesson, users resume from their last position within 2 seconds of page load
- **SC-004**: Users receive point rewards within 1 second of reaching 80% video completion
- **SC-005**: 95%+ of videos marked complete result in successful point awards (5% tolerance for network/system issues with retry)
- **SC-006**: Video player renders correctly and is fully functional on desktop, tablet (768px+), and mobile (480px+) devices
- **SC-007**: Free users attempting to access premium content see upgrade prompts instead of videos 100% of the time
- **SC-008**: Keyboard shortcuts respond within 200ms of key press for 99%+ of interactions
- **SC-009**: Unavailable or deleted YouTube videos display error messages instead of broken players 100% of the time
- **SC-010**: Progress tracking handles 1000+ concurrent users watching videos without performance degradation
- **SC-011**: Database write operations for progress saves are optimized through debouncing, resulting in maximum 6 writes per minute per user
- **SC-012**: Video completion rate (users who complete videos they start) increases by 25% compared to baseline without resume functionality
- **SC-013**: 90%+ of users successfully complete their first video and receive points on first attempt
- **SC-014**: Average watch time per user session increases by 30% with progress tracking compared to without

## Assumptions

- YouTube API and embed functionality remain available and stable
- YouTube videos assigned to lessons are publicly accessible or unlisted (not private)
- Users have stable internet connections sufficient for video streaming (minimum 1 Mbps)
- Browser supports modern JavaScript features required by react-player library
- Database can handle 10,000+ progress save operations per hour during peak usage
- The existing authentication system (JWT) is functional and provides user identity
- The existing subscription/payment system can identify premium vs. free users
- Point rewards system is already implemented and has an API for adding points to user accounts
- Lesson data includes YouTube video URL or video ID field
- Admin interface exists for managing lesson content and video URLs
- Mobile browsers support HTML5 video playback (iOS Safari, Chrome Mobile, etc.)
- System has logging infrastructure for debugging video playback issues
- Error monitoring system can alert admins when videos become unavailable
- Backend API framework supports debounced/batched write operations
- Frontend state management (React hooks/Zustand) can track player state
- UI component library (TailwindCSS) supports responsive video player layout

## Dependencies

- **react-player** library: YouTube video embedding and playback control
- **Existing authentication system**: User identity and JWT token validation
- **Existing subscription system**: Premium user status verification
- **Existing points/rewards system**: API for awarding points
- **Database schema**: UserProgress and LessonCompletion tables
- **Lesson management system**: Source of YouTube video URLs and metadata
- **Backend API**: Endpoints for progress save, load, and completion detection
- **Frontend routing**: Navigation to lesson pages
- **UI notification system**: Toast/modal components for completion feedback
- **Error handling infrastructure**: Graceful error display and logging
- **YouTube API**: Video metadata validation (optional but recommended)

## Out of Scope

- Custom video hosting or file uploads (YouTube only)
- Custom video player UI design (using YouTube's native controls)
- Video download functionality
- Picture-in-picture mode (future enhancement)
- Comments or notes on videos (future feature)
- Video recommendations or "Up Next" features (future feature)
- Social features (sharing watch progress, watch parties)
- Playlist creation across multiple lessons
- Video quality selection beyond what YouTube provides
- Subtitle or caption editing
- Video analytics beyond basic completion tracking
- Integration with video platforms other than YouTube
- Offline video caching for mobile apps
- Live streaming or webinar functionality
- Interactive video elements (quizzes embedded in video timeline)