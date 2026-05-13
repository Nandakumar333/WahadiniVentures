/**
 * URL Sanitization Utility
 * 
 * Provides functions to validate and sanitize YouTube video IDs and URLs
 * to prevent XSS attacks and ensure only valid YouTube content is embedded.
 */

/**
 * Regular expression to validate YouTube video IDs
 * YouTube video IDs are exactly 11 characters long and contain only:
 * - Alphanumeric characters (a-z, A-Z, 0-9)
 * - Underscores (_)
 * - Hyphens (-)
 */
const YOUTUBE_VIDEO_ID_REGEX = /^[a-zA-Z0-9_-]{11}$/;

/**
 * Validates if a string is a valid YouTube video ID
 * 
 * @param videoId - The video ID to validate
 * @returns true if the video ID is valid, false otherwise
 * 
 * @example
 * ```typescript
 * isValidYouTubeVideoId('dQw4w9WgXcQ'); // true
 * isValidYouTubeVideoId('invalid'); // false
 * isValidYouTubeVideoId('<script>alert("xss")</script>'); // false
 * ```
 */
export function isValidYouTubeVideoId(videoId: string | null | undefined): boolean {
  if (!videoId || typeof videoId !== 'string') {
    return false;
  }

  return YOUTUBE_VIDEO_ID_REGEX.test(videoId);
}

/**
 * Sanitizes a YouTube video ID by validating it against the expected format
 * Returns null if the video ID is invalid or potentially malicious
 * 
 * @param videoId - The video ID to sanitize
 * @returns The sanitized video ID or null if invalid
 * 
 * @example
 * ```typescript
 * sanitizeYouTubeVideoId('dQw4w9WgXcQ'); // 'dQw4w9WgXcQ'
 * sanitizeYouTubeVideoId('invalid'); // null
 * sanitizeYouTubeVideoId('<script>alert("xss")</script>'); // null
 * ```
 */
export function sanitizeYouTubeVideoId(videoId: string | null | undefined): string | null {
  if (!isValidYouTubeVideoId(videoId)) {
    console.warn('[URL Sanitizer] Invalid YouTube video ID detected:', videoId);
    return null;
  }

  return videoId as string;
}

/**
 * Extracts and validates a YouTube video ID from a full YouTube URL
 * Supports various YouTube URL formats:
 * - https://www.youtube.com/watch?v=VIDEO_ID
 * - https://youtu.be/VIDEO_ID
 * - https://www.youtube.com/embed/VIDEO_ID
 * 
 * @param url - The YouTube URL to extract the video ID from
 * @returns The extracted and sanitized video ID or null if invalid
 * 
 * @example
 * ```typescript
 * extractYouTubeVideoId('https://www.youtube.com/watch?v=dQw4w9WgXcQ'); // 'dQw4w9WgXcQ'
 * extractYouTubeVideoId('https://youtu.be/dQw4w9WgXcQ'); // 'dQw4w9WgXcQ'
 * extractYouTubeVideoId('invalid-url'); // null
 * ```
 */
export function extractYouTubeVideoId(url: string | null | undefined): string | null {
  if (!url || typeof url !== 'string') {
    return null;
  }

  try {
    const urlObj = new URL(url);
    let videoId: string | null = null;

    // Handle youtube.com/watch?v=VIDEO_ID
    if (urlObj.hostname.includes('youtube.com')) {
      videoId = urlObj.searchParams.get('v');
    }
    // Handle youtu.be/VIDEO_ID
    else if (urlObj.hostname === 'youtu.be') {
      videoId = urlObj.pathname.slice(1); // Remove leading slash
    }
    // Handle youtube.com/embed/VIDEO_ID
    else if (urlObj.pathname.includes('/embed/')) {
      const parts = urlObj.pathname.split('/');
      videoId = parts[parts.length - 1];
    }

    return sanitizeYouTubeVideoId(videoId);
  } catch (error) {
    // Invalid URL format
    console.warn('[URL Sanitizer] Invalid URL format:', url);
    return null;
  }
}

/**
 * Builds a safe YouTube embed URL from a video ID
 * Returns null if the video ID is invalid
 * 
 * @param videoId - The YouTube video ID
 * @returns A safe YouTube embed URL or null if invalid
 * 
 * @example
 * ```typescript
 * buildSafeYouTubeUrl('dQw4w9WgXcQ'); // 'https://www.youtube.com/embed/dQw4w9WgXcQ'
 * buildSafeYouTubeUrl('invalid'); // null
 * ```
 */
export function buildSafeYouTubeUrl(videoId: string | null | undefined): string | null {
  const sanitized = sanitizeYouTubeVideoId(videoId);
  if (!sanitized) {
    return null;
  }

  return `https://www.youtube.com/embed/${sanitized}`;
}

/**
 * Validates and sanitizes a YouTube video ID with detailed error information
 * Useful for form validation and user feedback
 * 
 * @param videoId - The video ID to validate
 * @returns Validation result with success flag and optional error message
 * 
 * @example
 * ```typescript
 * validateYouTubeVideoId('dQw4w9WgXcQ'); // { valid: true, videoId: 'dQw4w9WgXcQ' }
 * validateYouTubeVideoId('invalid'); // { valid: false, error: 'Invalid YouTube video ID format' }
 * ```
 */
export function validateYouTubeVideoId(
  videoId: string | null | undefined
): { valid: boolean; videoId?: string; error?: string } {
  if (!videoId) {
    return {
      valid: false,
      error: 'Video ID is required',
    };
  }

  if (typeof videoId !== 'string') {
    return {
      valid: false,
      error: 'Video ID must be a string',
    };
  }

  if (!YOUTUBE_VIDEO_ID_REGEX.test(videoId)) {
    return {
      valid: false,
      error: 'Invalid YouTube video ID format. Must be 11 characters containing only letters, numbers, hyphens, and underscores.',
    };
  }

  return {
    valid: true,
    videoId: videoId,
  };
}
