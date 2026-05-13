import axios from 'axios';
import type { AxiosInstance } from 'axios';
import type {
  TaskReviewRequestDto,
  DiscountCodeDto,
  CreateDiscountCodeDto,
  RedemptionLogDto,
  AdjustPointsRequestDto
} from '../types/admin.types';

/**
 * Admin API Service
 * Axios client configured with JWT authentication for /api/admin/* endpoints
 * All requests automatically include Authorization header from localStorage
 */
class AdminService {
  private readonly client: AxiosInstance;

  constructor() {
    this.client = axios.create({
      baseURL: import.meta.env.VITE_API_URL || 'http://localhost:5171',
      headers: {
        'Content-Type': 'application/json'
      }
    });

    // Request interceptor - Add JWT token to all requests
    this.client.interceptors.request.use(
      (config) => {
        const token = localStorage.getItem('token');
        if (token) {
          config.headers.Authorization = `Bearer ${token}`;
        }
        return config;
      },
      (error) => {
        return Promise.reject(error);
      }
    );

    // Response interceptor - Handle authentication errors
    this.client.interceptors.response.use(
      (response) => response,
      (error) => {
        if (error.response?.status === 401) {
          // Token expired or invalid - clear and redirect to login
          localStorage.removeItem('token');
          localStorage.removeItem('user');
          globalThis.location.href = '/auth/login';
        }
        return Promise.reject(error);
      }
    );
  }

  // Dashboard Stats
  async getDashboardStats() {
    const response = await this.client.get('/api/admin/stats');
    return response.data;
  }

  // Audit Logs
  async getAuditLogs(filters: any) {
    const response = await this.client.get('/api/admin/audit-logs', {
      params: filters
    });
    return response.data;
  }

  async getAuditLogById(id: string) {
    const response = await this.client.get(`/api/admin/audit-logs/${id}`);
    return response.data;
  }

  async getResourceHistory(resourceType: string, resourceId: string) {
    const response = await this.client.get(
      `/api/admin/audit-logs/resource/${resourceType}/${resourceId}`
    );
    return response.data;
  }

  // Notifications
  async getUnreadNotifications(pageNumber = 1, pageSize = 20) {
    const response = await this.client.get('/api/admin/notifications/unread', {
      params: { pageNumber, pageSize }
    });
    return response.data;
  }

  async getUnreadCount() {
    const response = await this.client.get('/api/admin/notifications/unread-count');
    return response.data;
  }

  async markNotificationAsRead(id: string) {
    const response = await this.client.put(`/api/admin/notifications/${id}/mark-read`);
    return response.data;
  }

  async markAllNotificationsAsRead() {
    const response = await this.client.put('/api/admin/notifications/mark-all-read');
    return response.data;
  }

  /**
   * Retrieves pending task submissions for review
   * T054: US2 - Task Review Workflow
   */
  async getPendingTasks(filters?: {
    dateFrom?: string;
    dateTo?: string;
    courseId?: string;
    pageNumber?: number;
    pageSize?: number;
  }) {
    const params = new URLSearchParams();
    if (filters?.dateFrom) params.append('dateFrom', filters.dateFrom);
    if (filters?.dateTo) params.append('dateTo', filters.dateTo);
    if (filters?.courseId) params.append('courseId', filters.courseId);
    if (filters?.pageNumber) params.append('pageNumber', filters.pageNumber.toString());
    if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString());

    const response = await this.client.get(`/api/admin/tasks/pending?${params.toString()}`);
    return response.data;
  }

  /**
   * Reviews a task submission (approve/reject)
   * T055: US2 - Task Review Workflow
   */
  async reviewTask(submissionId: string, review: TaskReviewRequestDto): Promise<void> {
    await this.client.post(`/api/admin/tasks/${submissionId}/review`, review);
  }

  /**
   * Retrieves paginated list of users with filters
   * T077: US3 - User Account Management
   */
  async getUsers(filters?: {
    searchTerm?: string;
    role?: number;
    isActive?: boolean;
    isBanned?: boolean;
    emailConfirmed?: boolean;
    hasActiveSubscription?: boolean;
    pageNumber?: number;
    pageSize?: number;
    sortBy?: string;
    sortDescending?: boolean;
  }) {
    const params = new URLSearchParams();
    if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm);
    if (filters?.role !== undefined) params.append('role', filters.role.toString());
    if (filters?.isActive !== undefined) params.append('isActive', filters.isActive.toString());
    if (filters?.isBanned !== undefined) params.append('isBanned', filters.isBanned.toString());
    if (filters?.emailConfirmed !== undefined) params.append('emailConfirmed', filters.emailConfirmed.toString());
    if (filters?.hasActiveSubscription !== undefined) params.append('hasActiveSubscription', filters.hasActiveSubscription.toString());
    if (filters?.pageNumber) params.append('pageNumber', filters.pageNumber.toString());
    if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters?.sortBy) params.append('sortBy', filters.sortBy);
    if (filters?.sortDescending !== undefined) params.append('sortDescending', filters.sortDescending.toString());

    const response = await this.client.get(`/api/admin/users?${params.toString()}`);
    return response.data;
  }

  /**
   * Retrieves detailed information for a specific user
   * T077: US3 - User Account Management
   */
  async getUserById(userId: string) {
    const response = await this.client.get(`/api/admin/users/${userId}`);
    return response.data;
  }

  /**
   * Updates a user's role
   * T078: US3 - User Account Management
   */
  async updateUserRole(userId: string, data: { role: number; reason?: string }) {
    await this.client.put(`/api/admin/users/${userId}/role`, data);
  }

  /**
   * Bans a user account
   * T078: US3 - User Account Management
   */
  async banUser(userId: string, data: { reason: string }) {
    await this.client.post(`/api/admin/users/${userId}/ban`, data);
  }

  /**
   * Unbans a user account
   * T078: US3 - User Account Management
   */
  async unbanUser(userId: string) {
    await this.client.post(`/api/admin/users/${userId}/unban`);
  }

  /**
   * Retrieves paginated list of courses with filters
   * T124: US4 - Course Content Management
   */
  async getCourses(filters?: {
    pageNumber?: number;
    pageSize?: number;
    categoryId?: string;
    isPublished?: boolean;
    searchTerm?: string;
  }) {
    const params = new URLSearchParams();
    if (filters?.pageNumber) params.append('pageNumber', filters.pageNumber.toString());
    if (filters?.pageSize) params.append('pageSize', filters.pageSize.toString());
    if (filters?.categoryId) params.append('categoryId', filters.categoryId);
    if (filters?.isPublished !== undefined) params.append('isPublished', filters.isPublished.toString());
    if (filters?.searchTerm) params.append('searchTerm', filters.searchTerm);

    const response = await this.client.get(`/api/admin/courses?${params.toString()}`);
    return response.data;
  }

  /**
   * Creates a new course
   * T124: US4 - Course Content Management
   */
  async createCourse(data: {
    title: string;
    description: string;
    categoryId: string;
    thumbnailUrl?: string;
    difficulty: number;
    isPremium: boolean;
    isPublished: boolean;
  }) {
    const response = await this.client.post('/api/admin/courses', data);
    return response.data;
  }

  /**
   * Updates an existing course
   * T124: US4 - Course Content Management
   */
  async updateCourse(courseId: string, data: {
    title: string;
    description: string;
    categoryId: string;
    thumbnailUrl?: string;
    difficulty: number;
    isPremium: boolean;
    isPublished: boolean;
  }) {
    await this.client.put(`/api/admin/courses/${courseId}`, data);
  }

  /**
   * Deletes a course (soft delete)
   * T124: US4 - Course Content Management
   */
  async deleteCourse(courseId: string) {
    await this.client.delete(`/api/admin/courses/${courseId}`);
  }

  // US5: Discount Management (T151-T152)

  /**
   * Gets all discount codes with optional status filter
   * T151: US5 - Reward System Management
   */
  async getDiscountCodes(statusFilter?: string): Promise<DiscountCodeDto[]> {
    const params = statusFilter ? { statusFilter } : {};
    const response = await this.client.get<DiscountCodeDto[]>('/api/admin/discounts', { params });
    return response.data;
  }

  /**
   * Creates a new discount code
   * T151: US5 - Reward System Management
   */
  async createDiscountCode(data: CreateDiscountCodeDto): Promise<string> {
    const response = await this.client.post<string>('/api/admin/discounts', data);
    return response.data;
  }

  /**
   * Gets redemption logs for a discount code
   * T152: US5 - Reward System Management
   */
  async getRedemptions(code: string, dateFrom?: string, dateTo?: string): Promise<RedemptionLogDto[]> {
    const params = { dateFrom, dateTo };
    const response = await this.client.get<RedemptionLogDto[]>(`/api/admin/discounts/${code}/redemptions`, { params });
    return response.data;
  }

  /**
   * Adjusts a user's point balance
   * T152: US5 - Reward System Management
   */
  async adjustUserPoints(userId: string, data: AdjustPointsRequestDto): Promise<{ newBalance: number }> {
    const response = await this.client.post<{ newBalance: number }>(`/api/admin/users/${userId}/points/adjust`, data);
    return response.data;
  }
}

// Export singleton instance
export const adminService = new AdminService();
export default adminService;
