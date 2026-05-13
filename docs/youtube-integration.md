# YouTube Video Integration Guide

**WahadiniCryptoQuest Platform** - Technical Documentation  
**Feature**: Course & Lesson Management System  
**Last Updated**: November 15, 2025

---

## Overview

This document provides technical details about YouTube video integration in the WahadiniCryptoQuest platform. Video lessons are a core feature of the crypto learning platform, with automated video ID extraction, validation, and progress tracking.

**Key Features**:
- Automatic video ID extraction from multiple URL formats
- Client-side and server-side validation
- Embedded video playback with progress tracking
- Automatic lesson completion detection

---

## Supported URL Formats

The platform accepts YouTube video URLs in the following formats and automatically extracts the 11-character video ID:

### Format 1: Standard Watch URL

**Pattern**: `https://www.youtube.com/watch?v={VIDEO_ID}`

**Examples**:
```
https://www.youtube.com/watch?v=dQw4w9WgXcQ
https://www.youtube.com/watch?v=9bZkp7q19f0
https://www.youtube.com/watch?v=jNQXAC9IVRw
```

**Extracted Video IDs**:
- `dQw4w9WgXcQ`
- `9bZkp7q19f0`
- `jNQXAC9IVRw`

**Additional Query Parameters** (ignored by extractor):
```
https://www.youtube.com/watch?v=dQw4w9WgXcQ&t=30s
https://www.youtube.com/watch?v=dQw4w9WgXcQ&list=PLrAXtmErZgOeiKm4sgNOknGvNjby9efdf
```

**Extraction Logic**: Captures value of `v=` query parameter

---

### Format 2: Shortened Share URL

**Pattern**: `https://youtu.be/{VIDEO_ID}`

**Examples**:
```
https://youtu.be/dQw4w9WgXcQ
https://youtu.be/9bZkp7q19f0
https://youtu.be/jNQXAC9IVRw
```

**Extracted Video IDs**:
- `dQw4w9WgXcQ`
- `9bZkp7q19f0`
- `jNQXAC9IVRw`

**With Timestamp** (ignored):
```
https://youtu.be/dQw4w9WgXcQ?t=30
```

**Extraction Logic**: Captures path segment after domain

---

### Format 3: Embed URL

**Pattern**: `https://www.youtube.com/embed/{VIDEO_ID}`

**Examples**:
```
https://www.youtube.com/embed/dQw4w9WgXcQ
https://www.youtube.com/embed/9bZkp7q19f0
```

**Extracted Video IDs**:
- `dQw4w9WgXcQ`
- `9bZkp7q19f0`

**Extraction Logic**: Captures path segment after `/embed/`

---

### Format 4: Direct Video ID

**Pattern**: `{VIDEO_ID}` (11 characters)

**Examples**:
```
dQw4w9WgXcQ
9bZkp7q19f0
jNQXAC9IVRw
```

**Validation**: Must be exactly 11 characters, alphanumeric (including `-` and `_`)

**Use Case**: Admin manually extracts video ID and enters directly

---

## Video ID Validation Rules

### Character Requirements

**Valid Characters**:
- Uppercase letters: `A-Z`
- Lowercase letters: `a-z`
- Numbers: `0-9`
- Hyphen: `-`
- Underscore: `_`

**Invalid Characters**:
- Spaces, special characters (`!@#$%^&*()`), unicode

### Length Requirements

**Exact Length**: 11 characters

**Valid Examples**:
```
dQw4w9WgXcQ  ✅ (11 chars, alphanumeric + underscore)
9bZkp7q19f0  ✅ (11 chars, alphanumeric)
jNQXAC9IVRw  ✅ (11 chars, alphanumeric)
a1-b2_c3d4e  ✅ (11 chars, includes hyphen and underscore)
```

**Invalid Examples**:
```
abc123       ❌ (6 chars, too short)
123-456-789  ❌ (11 chars but includes extra hyphens)
dQw4w9WgXcQ! ❌ (12 chars, invalid character)
dQw4w9WgXc   ❌ (10 chars, too short)
```

---

## Frontend Implementation

### YouTube Video ID Extraction

**File**: `frontend/src/utils/youtube.helpers.ts`

**Function**: `extractYouTubeVideoId(input: string): string | null`

**Logic**:
```typescript
export function extractYouTubeVideoId(input: string): string | null {
  // Remove whitespace
  const trimmedInput = input.trim();

  // Pattern 1: Standard watch URL - https://www.youtube.com/watch?v={VIDEO_ID}
  const watchMatch = trimmedInput.match(/(?:youtube\.com\/watch\?v=)([a-zA-Z0-9_-]{11})/);
  if (watchMatch) return watchMatch[1];

  // Pattern 2: Shortened URL - https://youtu.be/{VIDEO_ID}
  const shortMatch = trimmedInput.match(/(?:youtu\.be\/)([a-zA-Z0-9_-]{11})/);
  if (shortMatch) return shortMatch[1];

  // Pattern 3: Embed URL - https://www.youtube.com/embed/{VIDEO_ID}
  const embedMatch = trimmedInput.match(/(?:youtube\.com\/embed\/)([a-zA-Z0-9_-]{11})/);
  if (embedMatch) return embedMatch[1];

  // Pattern 4: Direct video ID (11 characters, alphanumeric + hyphen/underscore)
  const directMatch = trimmedInput.match(/^([a-zA-Z0-9_-]{11})$/);
  if (directMatch) return directMatch[1];

  // No match found
  return null;
}
```

**Usage**:
```typescript
import { extractYouTubeVideoId } from '@/utils/youtube.helpers';

const input = 'https://www.youtube.com/watch?v=dQw4w9WgXcQ';
const videoId = extractYouTubeVideoId(input); // Returns: "dQw4w9WgXcQ"

if (videoId) {
  // Valid video ID extracted
  console.log(`Video ID: ${videoId}`);
} else {
  // Invalid input
  console.error('Invalid YouTube URL or video ID');
}
```

### Validation Function

**Function**: `isValidYouTubeVideoId(videoId: string): boolean`

**Logic**:
```typescript
export function isValidYouTubeVideoId(videoId: string): boolean {
  // Check length (must be exactly 11 characters)
  if (videoId.length !== 11) return false;

  // Check characters (alphanumeric, hyphen, underscore only)
  const validPattern = /^[a-zA-Z0-9_-]{11}$/;
  return validPattern.test(videoId);
}
```

**Usage**:
```typescript
import { isValidYouTubeVideoId } from '@/utils/youtube.helpers';

const videoId = 'dQw4w9WgXcQ';
const isValid = isValidYouTubeVideoId(videoId); // Returns: true

if (isValid) {
  // Proceed with video embedding
} else {
  // Show error message
}
```

---

## Backend Implementation

### Server-Side Validation

**File**: `backend/src/WahadiniCryptoQuest.API/Validators/Course/CreateLessonValidator.cs`

**FluentValidation Rule**:
```csharp
public class CreateLessonValidator : AbstractValidator<CreateLessonDto>
{
    public CreateLessonValidator()
    {
        // ... other rules

        RuleFor(x => x.YouTubeVideoId)
            .NotEmpty()
            .WithMessage("YouTube video ID is required")
            .Length(11)
            .WithMessage("YouTube video ID must be exactly 11 characters")
            .Matches(@"^[a-zA-Z0-9_-]{11}$")
            .WithMessage("YouTube video ID must contain only alphanumeric characters, hyphens, and underscores");
    }
}
```

**Validation Errors**:
- **Empty**: "YouTube video ID is required"
- **Wrong Length**: "YouTube video ID must be exactly 11 characters"
- **Invalid Characters**: "YouTube video ID must contain only alphanumeric characters, hyphens, and underscores"

### API Response

**400 Bad Request** (validation failure):
```json
{
  "type": "https://tools.ietf.org/html/rfc7231#section-6.5.1",
  "title": "One or more validation errors occurred.",
  "status": 400,
  "errors": {
    "YouTubeVideoId": [
      "YouTube video ID must be exactly 11 characters"
    ]
  }
}
```

---

## Video Playback Implementation

### React Player Integration

**Component**: `frontend/src/components/lessons/VideoPlayer.tsx`

**Library**: `react-player` (supports YouTube, Vimeo, etc.)

**Installation**:
```bash
npm install react-player
```

**Basic Usage**:
```tsx
import ReactPlayer from 'react-player/youtube';

interface VideoPlayerProps {
  videoId: string;
  onProgress?: (progress: number) => void;
  onComplete?: () => void;
}

export const VideoPlayer: React.FC<VideoPlayerProps> = ({ 
  videoId, 
  onProgress, 
  onComplete 
}) => {
  const handleProgress = (state: { played: number }) => {
    const progressPercentage = state.played * 100;
    onProgress?.(progressPercentage);

    // Auto-complete at 90% watched
    if (progressPercentage >= 90) {
      onComplete?.();
    }
  };

  return (
    <ReactPlayer
      url={`https://www.youtube.com/watch?v=${videoId}`}
      controls
      width="100%"
      height="100%"
      onProgress={handleProgress}
      progressInterval={5000} // Track every 5 seconds
    />
  );
};
```

### Progress Tracking

**File**: `frontend/src/hooks/lessons/useVideoTracking.ts`

**Custom Hook**:
```typescript
export function useVideoTracking(lessonId: string) {
  const [progress, setProgress] = useState(0);
  const [isCompleted, setIsCompleted] = useState(false);

  const handleProgress = (percentage: number) => {
    setProgress(percentage);

    // Mark lesson as complete at 90% watched
    if (percentage >= 90 && !isCompleted) {
      setIsCompleted(true);
      completeLessonMutation.mutate({ lessonId, progress: percentage });
    }
  };

  return { progress, isCompleted, handleProgress };
}
```

**Usage**:
```tsx
const { progress, isCompleted, handleProgress } = useVideoTracking(lesson.id);

return (
  <VideoPlayer
    videoId={lesson.youtubeVideoId}
    onProgress={handleProgress}
    onComplete={() => console.log('Lesson completed!')}
  />
);
```

---

## Error Handling

### Invalid Video ID

**Frontend Validation** (immediate feedback):
```typescript
const handleYouTubeUrlChange = (value: string) => {
  const videoId = extractYouTubeVideoId(value);

  if (!videoId) {
    setError('Invalid YouTube URL or video ID');
    return;
  }

  if (!isValidYouTubeVideoId(videoId)) {
    setError('Video ID must be 11 characters (alphanumeric, hyphens, underscores)');
    return;
  }

  // Valid - clear error and proceed
  setError(null);
  setValue('youtubeVideoId', videoId);
};
```

### Video Not Found (404)

**React Player Error Handling**:
```tsx
<ReactPlayer
  url={`https://www.youtube.com/watch?v=${videoId}`}
  controls
  onError={(error) => {
    console.error('Video playback error:', error);
    toast.error('Unable to load video. The video may have been removed or is private.');
  }}
/>
```

**Common Causes**:
- Video deleted by creator
- Video set to private or unlisted
- Invalid video ID (typo)
- Region-restricted content

### Network Errors

**Retry Logic**:
```tsx
const [retryCount, setRetryCount] = useState(0);
const MAX_RETRIES = 3;

const handleError = (error: any) => {
  if (retryCount < MAX_RETRIES) {
    setTimeout(() => {
      setRetryCount(prev => prev + 1);
    }, 2000); // Retry after 2 seconds
  } else {
    toast.error('Failed to load video after multiple attempts. Please check your connection.');
  }
};
```

---

## Performance Optimization

### Lazy Loading

**Defer video loading until visible**:
```tsx
import { lazy, Suspense } from 'react';

const ReactPlayer = lazy(() => import('react-player/youtube'));

export const VideoPlayer: React.FC<VideoPlayerProps> = (props) => {
  return (
    <Suspense fallback={<VideoPlayerSkeleton />}>
      <ReactPlayer {...props} />
    </Suspense>
  );
};
```

### Thumbnail Preview

**Show YouTube thumbnail before playback**:
```tsx
const thumbnailUrl = `https://img.youtube.com/vi/${videoId}/maxresdefault.jpg`;

{!isPlaying && (
  <img
    src={thumbnailUrl}
    alt="Video thumbnail"
    onClick={() => setIsPlaying(true)}
    className="cursor-pointer"
  />
)}
```

**Thumbnail Sizes**:
- `maxresdefault.jpg` - 1280x720 (highest quality)
- `hqdefault.jpg` - 480x360 (high quality)
- `mqdefault.jpg` - 320x180 (medium quality)
- `default.jpg` - 120x90 (thumbnail)

---

## Testing

### Unit Tests - Video ID Extraction

**File**: `frontend/src/utils/__tests__/youtube.helpers.test.ts`

```typescript
import { extractYouTubeVideoId, isValidYouTubeVideoId } from '../youtube.helpers';

describe('extractYouTubeVideoId', () => {
  it('should extract video ID from standard watch URL', () => {
    const result = extractYouTubeVideoId('https://www.youtube.com/watch?v=dQw4w9WgXcQ');
    expect(result).toBe('dQw4w9WgXcQ');
  });

  it('should extract video ID from shortened URL', () => {
    const result = extractYouTubeVideoId('https://youtu.be/dQw4w9WgXcQ');
    expect(result).toBe('dQw4w9WgXcQ');
  });

  it('should extract video ID from embed URL', () => {
    const result = extractYouTubeVideoId('https://www.youtube.com/embed/dQw4w9WgXcQ');
    expect(result).toBe('dQw4w9WgXcQ');
  });

  it('should accept direct video ID', () => {
    const result = extractYouTubeVideoId('dQw4w9WgXcQ');
    expect(result).toBe('dQw4w9WgXcQ');
  });

  it('should return null for invalid input', () => {
    const result = extractYouTubeVideoId('invalid-url');
    expect(result).toBeNull();
  });
});

describe('isValidYouTubeVideoId', () => {
  it('should validate correct video ID', () => {
    expect(isValidYouTubeVideoId('dQw4w9WgXcQ')).toBe(true);
  });

  it('should reject video ID with wrong length', () => {
    expect(isValidYouTubeVideoId('abc123')).toBe(false);
  });

  it('should reject video ID with invalid characters', () => {
    expect(isValidYouTubeVideoId('dQw4w9WgXcQ!')).toBe(false);
  });
});
```

### Backend Validation Tests

**File**: `backend/tests/WahadiniCryptoQuest.API.Tests/ValidatorTests.cs`

```csharp
[Fact]
public void CreateLessonValidator_ValidYouTubeVideoId_PassesValidation()
{
    // Arrange
    var validator = new CreateLessonValidator();
    var dto = new CreateLessonDto
    {
        Title = "Test Lesson",
        YouTubeVideoId = "dQw4w9WgXcQ", // Valid 11-char ID
        Duration = 10
    };

    // Act
    var result = validator.Validate(dto);

    // Assert
    Assert.True(result.IsValid);
}

[Theory]
[InlineData("abc123")] // Too short
[InlineData("dQw4w9WgXcQ!")] // Invalid character
[InlineData("")] // Empty
public void CreateLessonValidator_InvalidYouTubeVideoId_FailsValidation(string videoId)
{
    // Arrange
    var validator = new CreateLessonValidator();
    var dto = new CreateLessonDto
    {
        Title = "Test Lesson",
        YouTubeVideoId = videoId,
        Duration = 10
    };

    // Act
    var result = validator.Validate(dto);

    // Assert
    Assert.False(result.IsValid);
    Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateLessonDto.YouTubeVideoId));
}
```

---

## Security Considerations

### Content Security Policy (CSP)

**Allow YouTube iframe embedding**:
```html
<meta 
  http-equiv="Content-Security-Policy" 
  content="frame-src 'self' https://www.youtube.com"
>
```

### HTTPS Only

**Enforce secure connections**:
- All YouTube URLs use `https://` protocol
- Platform enforces HTTPS in production
- Mixed content warnings avoided

### User-Generated Content

**Risk**: Admin enters malicious URL disguised as YouTube link

**Mitigation**:
- Server-side validation rejects non-YouTube URLs
- Client-side regex validation ensures YouTube domain
- React Player sanitizes embed URLs internally

---

## API Reference

### Create Lesson with YouTube Video

**Endpoint**: `POST /api/courses/{courseId}/lessons`

**Request Body**:
```json
{
  "title": "Introduction to Bitcoin",
  "description": "Learn the basics of Bitcoin",
  "youtubeVideoId": "dQw4w9WgXcQ",
  "duration": 15,
  "rewardPoints": 25,
  "isPremium": false
}
```

**Response** (201 Created):
```json
{
  "id": "uuid",
  "title": "Introduction to Bitcoin",
  "description": "Learn the basics of Bitcoin",
  "youtubeVideoId": "dQw4w9WgXcQ",
  "duration": 15,
  "orderIndex": 1,
  "isPremium": false,
  "rewardPoints": 25,
  "createdAt": "2025-11-15T10:00:00Z"
}
```

**Validation Errors** (400 Bad Request):
```json
{
  "errors": {
    "YouTubeVideoId": [
      "YouTube video ID must be exactly 11 characters"
    ]
  }
}
```

---

## Troubleshooting

### Video ID Extraction Fails

**Problem**: Admin enters valid YouTube URL but system shows "Invalid video ID" error

**Solutions**:
1. Check for extra whitespace before/after URL
2. Verify URL format matches supported patterns
3. Test with direct 11-character video ID
4. Check browser console for JavaScript errors

### Video Doesn't Play

**Problem**: Video player shows black screen or error

**Causes & Solutions**:
1. **Video is private/unlisted**: Change video privacy to Public in YouTube Studio
2. **Video was deleted**: Verify video still exists on YouTube
3. **Region restriction**: Video may not be available in user's location
4. **Embedding disabled**: Creator must enable "Allow embedding" in YouTube settings
5. **Network issue**: Check internet connection, try refreshing page

### Progress Not Saving

**Problem**: User watches video but progress doesn't update

**Solutions**:
1. Verify user is logged in (JWT token valid)
2. Check network tab for failed API requests
3. Ensure `onProgress` callback is firing (check console logs)
4. Verify backend lesson completion endpoint is working
5. Test with different video to rule out video-specific issue

---

## Future Enhancements

### Planned Features

1. **Video Chapters**: Allow admins to define chapter markers with timestamps
2. **Subtitles/Captions**: Automatically load YouTube captions if available
3. **Playback Speed Control**: UI for adjusting video speed (0.5x, 1x, 1.5x, 2x)
4. **Video Quality Selection**: Allow users to choose resolution (360p, 720p, 1080p)
5. **Offline Download**: Download videos for offline viewing (premium feature)
6. **Video Analytics**: Track watch time, completion rate, rewatch frequency
7. **Interactive Quizzes**: Pause video at specific points for quiz questions
8. **Multi-Platform Support**: Support Vimeo, Dailymotion, or self-hosted videos

### Under Consideration

1. **Live Streaming**: Integrate YouTube Live for real-time courses
2. **Video Annotations**: Admin-defined clickable overlays
3. **Watch Parties**: Synchronized video viewing for groups
4. **Auto-Generated Transcripts**: AI-powered transcription for SEO and accessibility

---

## References

**External Documentation**:
- [YouTube IFrame API](https://developers.google.com/youtube/iframe_api_reference)
- [react-player Documentation](https://github.com/cookpete/react-player)
- [YouTube Video ID Format](https://webapps.stackexchange.com/questions/54443/format-for-id-of-youtube-video)

**Internal Resources**:
- [Admin Course Management Guide](./admin-course-management-guide.md)
- [API Documentation](/swagger)
- [Frontend Component Library](/storybook)

---

## Support

**Technical Questions**:
- GitHub Issues: Tag with `youtube-integration`
- Email: tech-support@wahadinicryptoquest.com

**Contributing**:
- Submit PRs for improved video ID extraction logic
- Report bugs in video playback
- Suggest new video platforms to support

---

## Changelog

### Version 1.0 (November 2025)
- Initial YouTube integration documentation
- Video ID extraction logic detailed
- Validation rules documented
- Error handling strategies included
- Testing examples provided
