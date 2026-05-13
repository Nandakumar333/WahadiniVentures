// API service exports
export { ApiClient } from './client';

// Service exports
export { courseService } from './course.service';
export { lessonService } from './lesson.service';
export { rewardService, formatPoints, getTransactionColor, getTransactionIcon, formatRelativeTime } from './reward.service';

// Create and export default client instance
import { ApiClient } from './client';

const apiClient = new ApiClient({
  baseURL: import.meta.env.VITE_API_BASE_URL || 'http://localhost:5171/api',
  timeout: 30000,
  withCredentials: true,
});

export default apiClient;