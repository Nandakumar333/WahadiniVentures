import { ApiClient } from '../api/client';
import type { ProgressDto, UpdateProgressDto, UpdateProgressResultDto } from '@/types/progress';

class ProgressService {
  private apiClient: ApiClient;

  constructor() {
    this.apiClient = new ApiClient();
  }

  /**
   * Get the current user's progress for a lesson
   * @param lessonId - The lesson ID
   * @returns Progress data or null if no progress exists
   */
  async getProgress(lessonId: string): Promise<ProgressDto | null> {
    try {
      const response = await this.apiClient.get<ProgressDto | null>(
        `/lessons/${lessonId}/progress`
      );
      return response.data ?? null;
    } catch (error) {
      console.error('Error fetching progress:', error);
      throw error;
    }
  }

  /**
   * Update the current user's progress for a lesson
   * @param lessonId - The lesson ID
   * @param watchPosition - Current watch position in seconds
   * @returns Update result with completion status
   */
  async updateProgress(
    lessonId: string,
    watchPosition: number
  ): Promise<UpdateProgressResultDto> {
    try {
      const dto: UpdateProgressDto = {
        watchPosition,
      };

      console.log(`[Progress Service] Calling PUT /lessons/${lessonId}/progress`, dto);

      const response = await this.apiClient.put<UpdateProgressResultDto>(
        `/lessons/${lessonId}/progress`,
        dto
      );

      console.log(`[Progress Service] Response:`, response);

      // The response is wrapped in ApiResponse, get the data
      if (!response || !response.data) {
        throw new Error('No response received');
      }
      return response.data;
    } catch (error: any) {
      console.error('[Progress Service] Error updating progress:', {
        error: error.message,
        response: error.response?.data,
        status: error.response?.status,
        lessonId,
        watchPosition
      });
      throw error;
    }
  }
}

// Export singleton instance
export const progressService = new ProgressService();
export default progressService;
