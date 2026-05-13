/**
 * Time formatting utilities for video playback
 */

/**
 * Formats seconds into human-readable time string
 * @param seconds - The number of seconds to format
 * @returns Formatted time string in MM:SS or HH:MM:SS format
 * @example
 * formatSeconds(65) // "01:05"
 * formatSeconds(3665) // "1:01:05"
 */
export function formatSeconds(seconds: number): string {
  // Handle edge cases
  if (typeof seconds !== 'number' || isNaN(seconds) || !isFinite(seconds)) {
    return '00:00';
  }

  // Handle negative numbers
  if (seconds < 0) {
    return '00:00';
  }

  // Round to nearest second
  const totalSeconds = Math.floor(seconds);
  
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const secs = totalSeconds % 60;

  // Format based on duration
  if (hours > 0) {
    // HH:MM:SS format
    return `${hours}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  } else {
    // MM:SS format with padded minutes
    return `${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`;
  }
}

/**
 * Formats seconds into a more verbose description
 * @param seconds - The number of seconds to format
 * @returns Formatted time string like "5 minutes 30 seconds"
 */
export function formatSecondsVerbose(seconds: number): string {
  if (typeof seconds !== 'number' || isNaN(seconds) || !isFinite(seconds) || seconds < 0) {
    return '0 seconds';
  }

  const totalSeconds = Math.floor(seconds);
  const hours = Math.floor(totalSeconds / 3600);
  const minutes = Math.floor((totalSeconds % 3600) / 60);
  const secs = totalSeconds % 60;

  const parts: string[] = [];
  
  if (hours > 0) {
    parts.push(`${hours} hour${hours !== 1 ? 's' : ''}`);
  }
  if (minutes > 0) {
    parts.push(`${minutes} minute${minutes !== 1 ? 's' : ''}`);
  }
  if (secs > 0 || parts.length === 0) {
    parts.push(`${secs} second${secs !== 1 ? 's' : ''}`);
  }

  return parts.join(' ');
}

/**
 * Calculates the percentage of video watched
 * @param watchedSeconds - Seconds watched
 * @param totalSeconds - Total video duration
 * @returns Percentage (0-100)
 */
export function calculateWatchPercentage(watchedSeconds: number, totalSeconds: number): number {
  if (totalSeconds <= 0 || typeof watchedSeconds !== 'number' || typeof totalSeconds !== 'number' || isNaN(watchedSeconds) || isNaN(totalSeconds)) {
    return 0;
  }
  
  const percentage = (watchedSeconds / totalSeconds) * 100;
  return Math.min(Math.max(percentage, 0), 100); // Clamp between 0-100
}
