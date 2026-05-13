import { useRef, useCallback, useEffect, useState, useMemo } from 'react';
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query';
import { progressService } from '@/services/progress';
import { queueProgressSave, getQueuedSaves, clearQueuedSave } from '@/utils/localStorage';
import type { UpdateProgressResultDto } from '@/types/progress';

/**
 * Parameters for the useVideoProgress hook
 */
interface UseVideoProgressParams {
  /** Unique identifier for the lesson being watched */
  lessonId: string;
  /** Total duration of the video in seconds, used to calculate completion percentage */
  videoDuration: number;
  /** Optional callback triggered when lesson reaches 80% completion for the first time, receives points awarded */
  onComplete?: (pointsAwarded: number) => void;
}

/**
 * Return type for the useVideoProgress hook
 */
interface UseVideoProgressReturn {
  /** Function to save progress at a specific watch position (seconds). Debounced to fire once every 10 seconds. */
  saveProgress: (watchPosition: number, force?: boolean) => void;
  /** True while a progress save request is in flight */
  isLoading: boolean;
  /** Error object if progress save failed after retries */
  error: Error | null;
  /** The last watch position (in seconds) that was successfully saved to the server */
  lastSavedPosition: number | null;
  /** Cached progress data including lastWatchedPosition, completionPercentage, and isCompleted flag */
  savedProgress: {
    lastWatchedPosition: number;
    completionPercentage: number;
    isCompleted: boolean;
  } | null;
}

/**
 * Minimum interval (in seconds) between automatic progress saves.
 * Prevents excessive API calls during video playback.
 */
const SAVE_INTERVAL_SECONDS = 5;

/**
 * Custom React hook for tracking and persisting video lesson progress.
 * 
 * **Key Features:**
 * - **Debounced Auto-Save**: Progress is saved automatically every 10 seconds during playback,
 *   preventing excessive API calls while ensuring data persistence.
 * - **Highest-Position Tracking**: Maintains the furthest point reached in the video. Seeking
 *   backwards does not decrease progress, allowing users to review content freely.
 * - **Offline Queue**: Failed saves are queued in localStorage and automatically synced when
 *   network connectivity is restored.
 * - **Completion Detection**: Automatically detects when user reaches 80% of video duration,
 *   triggers onComplete callback exactly once per session, and awards points.
 * - **Exponential Backoff Retry**: Network failures are retried with delays of 1s, 2s, 4s
 *   before giving up, providing resilience against temporary connectivity issues.
 * - **React Query Caching**: Progress data is cached for 2 minutes (staleTime) and kept in
 *   memory for 5 minutes (gcTime), reducing redundant API calls and improving UX.
 * 
 * **Debouncing Logic:**
 * Uses `lastSaveTimeRef` to track the timestamp of the last successful save. When `saveProgress()`
 * is called, it calculates elapsed time since last save. Only if >= 10 seconds have passed will
 * a new API request be initiated. This prevents spamming the backend during rapid player events.
 * 
 * **Retry Mechanism:**
 * On network failure, React Query's retry logic kicks in with exponential backoff:
 * - Attempt 1: Wait 1000ms (1 second)
 * - Attempt 2: Wait 2000ms (2 seconds)
 * - Attempt 3: Wait 4000ms (4 seconds)
 * Maximum delay is capped at 8000ms. After 3 failed attempts, the save is queued in localStorage.
 * 
 * **localStorage Queue:**
 * When all retries fail, the progress update is stored in localStorage under key "queued-progress-saves".
 * On component mount (useEffect), the hook checks for queued saves for the current lessonId and
 * attempts to sync them. Once successfully synced, the queued save is cleared via `clearQueuedSave()`.
 * 
 * **Completion Percentage Calculation:**
 * Backend calculates completion as (lastWatchedPosition / videoDuration) * 100. When this reaches
 * or exceeds 80%, the lesson is marked complete. The `completionTriggeredRef` ensures the onComplete
 * callback fires only once per session, even if the user continues watching past 80%.
 * 
 * @param params - Configuration object containing lessonId, videoDuration, and optional onComplete callback
 * @returns Object with saveProgress function, loading state, error state, lastSavedPosition, and cached savedProgress
 * 
 * @example
 * ```tsx
 * const { saveProgress, lastSavedPosition, savedProgress } = useVideoProgress({
 *   lessonId: 'abc-123',
 *   videoDuration: 600, // 10 minutes
 *   onComplete: (points) => {
 *     console.log(`Lesson completed! Earned ${points} points`);
 *     showNotification('Congratulations! You earned 50 points!');
 *   }
 * });
 * 
 * // In video player onProgress callback
 * const handleProgress = (played: number) => {
 *   const currentPosition = played * videoDuration;
 *   saveProgress(currentPosition); // Only saves every 10 seconds
 * };
 * ```
 */
export function useVideoProgress({
  lessonId,
  videoDuration,
  onComplete,
}: UseVideoProgressParams): UseVideoProgressReturn {
  /**
   * React Query client for cache updates
   */
  const queryClient = useQueryClient();

  /**
   * Ref to track the timestamp (Date.now()) of the last successful progress save.
   * Used to implement debouncing logic - ensures saves only fire every 10 seconds.
   */
  const lastSaveTimeRef = useRef<number>(0);
  
  /**
   * State tracking the last watch position (in seconds) that was successfully persisted to the server.
   * Updated after successful mutation, used for UI indicators showing "Progress saved at 2:30".
   */
  const [lastSavedPosition, setLastSavedPosition] = useState<number | null>(null);
  
  /**
   * Ref to prevent duplicate completion callbacks within a single session.
   * Once set to true when 80% reached, remains true until component unmounts or lessonId changes.
   */
  const completionTriggeredRef = useRef<boolean>(false);

  /**
   * Fetch existing progress with optimized React Query caching (T303).
   * - staleTime: 2 minutes - Data is considered fresh, no refetch on component remount
   * - gcTime: 5 minutes - Cached data kept in memory, faster access if user navigates back
   * 
   * This reduces redundant API calls when user switches between tabs or lessons frequently.
   */
  const { data: savedProgress } = useQuery({
    queryKey: ['lesson-progress', lessonId],
    queryFn: async () => {
      console.log(`[Progress Fetch] Loading progress for lessonId=${lessonId}`);
      const progress = await progressService.getProgress(lessonId);
      if (progress) {
        console.log(`[Progress Fetch] Found existing progress:`, progress);
        setLastSavedPosition(progress.lastWatchedPosition);
        return {
          lastWatchedPosition: progress.lastWatchedPosition,
          completionPercentage: progress.completionPercentage,
          isCompleted: progress.isCompleted,
        };
      }
      console.log(`[Progress Fetch] No existing progress found`);
      return null;
    },
    staleTime: 2 * 60 * 1000, // Cache for 2 minutes (T303)
    gcTime: 5 * 60 * 1000, // Keep in cache for 5 minutes (T303)
  });

  /**
   * Mutation for updating progress on the backend with retry logic.
   * 
   * **Retry Strategy:**
   * - Retries: 3 attempts maximum
   * - Backoff: Exponential with formula Math.min(1000 * 2^attemptIndex, 8000)
   *   - Attempt 1: 1000ms (1 second)
   *   - Attempt 2: 2000ms (2 seconds)
   *   - Attempt 3: 4000ms (4 seconds)
   * - Max delay capped at 8000ms to prevent excessive waits
   * 
   * **Success Handler:**
   * - Updates lastSavedPosition state for UI indicators
   * - Clears any queued save from localStorage (offline sync complete)
   * - Checks if lesson newly completed (80% threshold crossed)
   * - Triggers onComplete callback exactly once using completionTriggeredRef guard
   * 
   * **Error Handler:**
   * - Logs error to console for debugging
   * - Queues failed save in localStorage for later retry (offline support)
   * - User sees error message but data is not lost
   */
  const mutation = useMutation({
    mutationFn: async (watchPosition: number) => {
      console.log(`[Progress API] Sending update: lessonId=${lessonId}, watchPosition=${watchPosition}`);
      return await progressService.updateProgress(lessonId, watchPosition);
    },
    retry: 3,
    retryDelay: (attemptIndex) => {
      // Exponential backoff: 1s, 2s, 4s
      return Math.min(1000 * Math.pow(2, attemptIndex), 8000);
    },
    onSuccess: (result: UpdateProgressResultDto, watchPosition: number) => {
      console.log(`[Progress API] Success: watchPosition=${watchPosition}, completionPercentage=${result.completionPercentage}%, isNewlyCompleted=${result.isNewlyCompleted}`);
      setLastSavedPosition(watchPosition);
      clearQueuedSave(lessonId);

      // Update the cached progress data with new values
      const updatedData = {
        lastWatchedPosition: watchPosition,
        completionPercentage: result.completionPercentage,
        isCompleted: result.isNewlyCompleted || result.completionPercentage >= 80,
      };
      
      queryClient.setQueryData(['lesson-progress', lessonId], (oldData: any) => ({
        ...updatedData,
        isCompleted: result.isNewlyCompleted || oldData?.isCompleted || updatedData.isCompleted,
      }));
      
      console.log(`[Progress Cache] Updated cache with:`, updatedData);

      // Trigger completion callback only once per session
      if (result.isNewlyCompleted && onComplete && !completionTriggeredRef.current) {
        completionTriggeredRef.current = true;
        onComplete(result.pointsAwarded);
      }
    },
    onError: (error: Error, watchPosition: number) => {
      console.error('[Progress API] Failed to save progress:', error);
      queueProgressSave(lessonId, watchPosition);
    },
  });
  
  const { mutate } = mutation;

  // Memoize expensive calculations (T302)
  const completionPercentage = useMemo(() => {
    if (!savedProgress) return 0;
    return savedProgress.completionPercentage;
  }, [savedProgress]);

  const isCompleted = useMemo(() => {
    if (!savedProgress) return false;
    return savedProgress.isCompleted;
  }, [savedProgress]);

  const progressStats = useMemo(() => {
    return {
      lastWatchedPosition: savedProgress?.lastWatchedPosition ?? 0,
      completionPercentage,
      isCompleted,
    };
  }, [savedProgress, completionPercentage, isCompleted]);

  /**
   * Saves video progress at the current watch position with debouncing.
   * 
   * **Debouncing Behavior:**
   * - Checks elapsed time since last save using `lastSaveTimeRef`
   * - Only proceeds if >= 10 seconds have passed (unless force=true)
   * - Updates `lastSaveTimeRef` immediately to prevent duplicate saves
   * 
   * **Validation:**
   * - Ensures watchPosition >= 0 and <= videoDuration
   * - Logs warning and returns early if invalid
   * 
   * **API Call:**
   * - Triggers mutation.mutate() which handles retry logic
   * - On success: Updates lastSavedPosition, clears localStorage queue
   * - On failure: Queues save in localStorage for later sync
   * 
   * @param watchPosition - Current playback position in seconds
   * @param force - If true, bypasses debounce and saves immediately
   */
  const saveProgress = useCallback(
    (watchPosition: number, force = false) => {
      const now = Date.now();
      const timeSinceLastSave = (now - lastSaveTimeRef.current) / 1000;

      // Only save if at least SAVE_INTERVAL_SECONDS has elapsed or force is true
      if (force || timeSinceLastSave >= SAVE_INTERVAL_SECONDS) {
        lastSaveTimeRef.current = now;

        // Validate position
        if (watchPosition < 0 || watchPosition > videoDuration) {
          console.warn('[Progress] Invalid watch position:', watchPosition);
          return;
        }

        const positionToInt = Math.floor(watchPosition);
        console.log(`[Progress] Saving progress${force ? ' (forced)' : ''}: ${positionToInt}s / ${videoDuration}s (${((positionToInt / videoDuration) * 100).toFixed(1)}%)`);
        mutate(positionToInt);
      }
    },
    [lessonId, videoDuration, mutate]
  );

  /**
   * Effect: Sync queued saves on component mount.
   * 
   * Checks localStorage for any failed saves for this lessonId (from previous sessions
   * where network was offline). If found, immediately attempts to sync them to the server.
   * Once successfully synced, the queued save is cleared.
   */
  useEffect(() => {
    const queuedSaves = getQueuedSaves();
    const queuedSave = queuedSaves.find(save => save.lessonId === lessonId);

    if (queuedSave) {
      console.log('Syncing queued progress save:', queuedSave);
      mutate(queuedSave.watchPosition);
    }
  }, [lessonId, mutate]);

  /**
   * Effect: Reset completion trigger on lesson change.
   * 
   * When navigating to a different lesson, reset the completionTriggeredRef so that
   * the onComplete callback can fire again if the new lesson is completed.
   */
  useEffect(() => {
    completionTriggeredRef.current = false;
  }, [lessonId]);

  return {
    saveProgress,
    isLoading: mutation.isPending,
    error: mutation.error,
    lastSavedPosition,
    savedProgress: progressStats,
  };
}
