import type { QueuedProgressSave } from '@/types/progress';

const PROGRESS_QUEUE_KEY = 'wahadini_progress_queue';

/**
 * Queue a progress save to localStorage for offline sync
 */
export function queueProgressSave(lessonId: string, watchPosition: number): void {
  try {
    const queue = getQueuedSaves();
    
    // Remove any existing save for this lesson
    const filteredQueue = queue.filter(item => item.lessonId !== lessonId);
    
    // Add new save
    filteredQueue.push({
      lessonId,
      watchPosition,
      timestamp: Date.now(),
    });

    localStorage.setItem(PROGRESS_QUEUE_KEY, JSON.stringify(filteredQueue));
  } catch (error) {
    console.error('Error queuing progress save:', error);
  }
}

/**
 * Get all queued progress saves
 */
export function getQueuedSaves(): QueuedProgressSave[] {
  try {
    const data = localStorage.getItem(PROGRESS_QUEUE_KEY);
    return data ? JSON.parse(data) : [];
  } catch (error) {
    console.error('Error getting queued saves:', error);
    return [];
  }
}

/**
 * Clear a specific queued save after successful sync
 */
export function clearQueuedSave(lessonId: string): void {
  try {
    const queue = getQueuedSaves();
    const filteredQueue = queue.filter(item => item.lessonId !== lessonId);
    localStorage.setItem(PROGRESS_QUEUE_KEY, JSON.stringify(filteredQueue));
  } catch (error) {
    console.error('Error clearing queued save:', error);
  }
}

/**
 * Clear all queued saves
 */
export function clearAllQueuedSaves(): void {
  try {
    localStorage.removeItem(PROGRESS_QUEUE_KEY);
  } catch (error) {
    console.error('Error clearing all queued saves:', error);
  }
}
