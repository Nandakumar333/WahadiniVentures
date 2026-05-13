// Authentication DTOs matching backend
export interface RegisterRequestDto {
  email: string;
  password: string;
  confirmPassword: string;
  firstName: string;
  lastName: string;
  acceptTerms: boolean;
  acceptMarketing: boolean;
}

export interface LoginRequestDto {
  email: string;
  password: string;
  rememberMe: boolean;
}

export interface AuthResponseDto {
  accessToken: string;
  refreshToken: string;
  expiresAt: string;
  user: UserDto;
}

export interface LoginResponse {
  success: boolean;
  accessToken: string;
  refreshToken: string;
  expiresIn: number;
  user?: UserDto;
  message?: string;
}

export interface UserDto {
  id: string;
  email: string;
  firstName: string;
  lastName: string;
  role: UserRole;
  emailConfirmed: boolean;
  createdAt: string;
  roles: string[];
  totalPoints?: number;
  avatar?: string;
}

export interface RefreshTokenRequestDto {
  refreshToken: string;
}

export interface ChangePasswordRequestDto {
  currentPassword: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ForgotPasswordRequestDto {
  email: string;
}

export interface ResetPasswordRequestDto {
  email: string;
  token: string;
  newPassword: string;
  confirmNewPassword: string;
}

export interface ConfirmEmailRequestDto {
  userId: string;
  token: string;
}

// User Role constants - Updated to include SuperAdmin (009-admin-dashboard)
export const UserRole = {
  Free: 0,
  Premium: 1,
  Admin: 2,
  SuperAdmin: 3,
} as const;

export type UserRole = typeof UserRole[keyof typeof UserRole];

// API Response wrapper
export interface ApiResponse<T> {
  data: T;
  success: boolean;
  message: string;
  errors: string[];
}

// Error response
export interface ApiError {
  message: string;
  errors: Record<string, string[]>;
  status: number;
}