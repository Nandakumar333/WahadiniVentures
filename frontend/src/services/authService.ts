import httpClient from '../lib/httpClient';
import type {
  AuthResponseDto,
  LoginRequestDto,
  LoginResponse,
  RegisterRequestDto,
  RefreshTokenRequestDto,
  ChangePasswordRequestDto,
  ForgotPasswordRequestDto,
  ResetPasswordRequestDto,
  ConfirmEmailRequestDto,
  UserDto,
} from '../types/api';

export class AuthService {
  /**
   * Register a new user
   */
  static async register(data: RegisterRequestDto): Promise<AuthResponseDto> {
    const response = await httpClient.post<AuthResponseDto>('/auth/register', data);
    return response.data;
  }

  /**
   * Login user
   */
  static async login(data: LoginRequestDto): Promise<LoginResponse> {
    const response = await httpClient.post<LoginResponse>('/auth/login', data);
    return response.data;
  }

  /**
   * Refresh access token
   */
  static async refreshToken(data: RefreshTokenRequestDto): Promise<AuthResponseDto> {
    const response = await httpClient.post<AuthResponseDto>('/auth/refresh', data);
    return response.data;
  }

  /**
   * Logout user
   */
  static async logout(): Promise<void> {
    const refreshToken = localStorage.getItem('refreshToken');
    if (refreshToken) {
      try {
        await httpClient.post('/auth/logout', { refreshToken });
      } catch {
        // Ignore errors during logout
      }
    }
    
    // Clear local storage
    localStorage.removeItem('accessToken');
    localStorage.removeItem('refreshToken');
    localStorage.removeItem('user');
  }

  /**
   * Get current user profile
   */
  static async getCurrentUser(): Promise<UserDto> {
    const response = await httpClient.get<UserDto>('/auth/me');
    return response.data;
  }

  /**
   * Change user password
   */
  static async changePassword(data: ChangePasswordRequestDto): Promise<void> {
    await httpClient.post('/auth/change-password', data);
  }

  /**
   * Request password reset
   */
  static async forgotPassword(data: ForgotPasswordRequestDto): Promise<void> {
    await httpClient.post('/auth/forgot-password', data);
  }

  /**
   * Reset password with token
   */
  static async resetPassword(data: ResetPasswordRequestDto): Promise<void> {
    await httpClient.post('/auth/reset-password', data);
  }

  /**
   * Confirm email address
   */
  static async confirmEmail(data: ConfirmEmailRequestDto): Promise<void> {
    await httpClient.post('/auth/confirm-email', data);
  }

  /**
   * Resend email confirmation
   */
  static async resendEmailConfirmation(email: string): Promise<void> {
    await httpClient.post('/auth/resend-confirmation', { email });
  }

  /**
   * Check if user is authenticated (has valid token)
   */
  static isAuthenticated(): boolean {
    const token = localStorage.getItem('accessToken');
    if (!token) return false;

    try {
      // Basic JWT expiration check (without verification)
      const payload = JSON.parse(atob(token.split('.')[1]));
      const currentTime = Date.now() / 1000;
      return payload.exp > currentTime;
    } catch {
      return false;
    }
  }

  /**
   * Get stored user from localStorage
   */
  static getStoredUser(): UserDto | null {
    const userStr = localStorage.getItem('user');
    if (!userStr) return null;
    
    try {
      return JSON.parse(userStr);
    } catch {
      return null;
    }
  }

  /**
   * Store authentication data
   */
  static storeAuthData(authResponse: AuthResponseDto): void {
    localStorage.setItem('accessToken', authResponse.accessToken);
    localStorage.setItem('refreshToken', authResponse.refreshToken);
    localStorage.setItem('user', JSON.stringify(authResponse.user));
  }
}