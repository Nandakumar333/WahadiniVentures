import { create } from 'zustand';
import { persist } from 'zustand/middleware';
import { AuthService } from '@/services/authService';
import type { UserDto, RegisterRequestDto } from '@/types/api';
import { UserRole } from '@/types/api';

interface AuthState {
  user: UserDto | null;
  accessToken: string | null;
  refreshToken: string | null;
  isAuthenticated: boolean;
  isLoading: boolean;
  error: string | null;
}

interface AuthActions {
  setUser: (user: UserDto | null) => void;
  setTokens: (accessToken: string, refreshToken: string) => void;
  login: (credentials: { email: string; password: string; rememberMe?: boolean }) => Promise<boolean>;
  register: (userData: RegisterRequestDto) => Promise<boolean>;
  verifyEmail: (data: { email: string; token: string }) => Promise<boolean>;
  logout: () => void;
  setLoading: (loading: boolean) => void;
  setError: (error: string | null) => void;
  // Role and subscription helpers
  hasRole: (role: number) => boolean;
  isPremium: () => boolean;
  isAdmin: () => boolean;
  isFree: () => boolean;
}

export const useAuthStore = create<AuthState & AuthActions>()(
  persist(
    (set, get) => ({
      // State
      user: null,
      accessToken: null,
      refreshToken: null,
      isAuthenticated: false,
      isLoading: false,
      error: null,

      // Actions
      setUser: (user) => {
        set({ user, isAuthenticated: !!user });
      },

      setTokens: (accessToken, refreshToken) => {
        set({ accessToken, refreshToken });
        // Store tokens in localStorage for API requests
        localStorage.setItem('accessToken', accessToken);
        localStorage.setItem('refreshToken', refreshToken);
      },

      login: async (credentials) => {
        try {
          set({ isLoading: true, error: null });
          
          const response = await AuthService.login({
            email: credentials.email,
            password: credentials.password,
            rememberMe: credentials.rememberMe || false,
          });

          // Check if login was successful
          if (response.success && response.user) {
            // Store tokens and user data
            get().setTokens(response.accessToken, response.refreshToken);
            get().setUser(response.user);
            return true;
          } else {
            set({ error: response.message || 'Login failed' });
            return false;
          }
        } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Login failed';
          set({ error: errorMessage });
          return false;
        } finally {
          set({ isLoading: false });
        }
      },

      register: async (userData) => {
        try {
          set({ isLoading: true, error: null });
          
          await AuthService.register(userData);
          
          return true;
        } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Registration failed';
          set({ error: errorMessage });
          return false;
        } finally {
          set({ isLoading: false });
        }
      },

      verifyEmail: async (data) => {
        try {
          set({ isLoading: true, error: null });
          
          await AuthService.confirmEmail({
            userId: data.email, // This should be userId, but we need to handle the mapping
            token: data.token,
          });
          
          // After email confirmation, user needs to login to get tokens
          return true;
        } catch (error) {
          const errorMessage = error instanceof Error ? error.message : 'Email verification failed';
          set({ error: errorMessage });
          return false;
        } finally {
          set({ isLoading: false });
        }
      },

      logout: async () => {
        try {
          // Call backend logout endpoint
          const refreshToken = get().refreshToken;
          if (refreshToken) {
            try {
              await fetch(`${import.meta.env.VITE_API_BASE_URL || 'http://localhost:5171/api'}/auth/logout`, {
                method: 'POST',
                headers: {
                  'Content-Type': 'application/json',
                  'Authorization': `Bearer ${get().accessToken}`
                },
                body: JSON.stringify({ refreshToken })
              });
            } catch {
              // Ignore backend errors during logout
            }
          }
        } finally {
          // Always clear local state and storage
          set({ 
            user: null, 
            accessToken: null, 
            refreshToken: null, 
            isAuthenticated: false,
            error: null
          });
          // Clear localStorage
          localStorage.removeItem('accessToken');
          localStorage.removeItem('refreshToken');
          localStorage.removeItem('user');
        }
      },

      setLoading: (isLoading) => {
        set({ isLoading });
      },

      setError: (error) => {
        set({ error });
      },

      // Role and subscription helper functions
      hasRole: (role) => {
        const { user } = get();
        return user ? user.role >= role : false;
      },

      isPremium: () => {
        const { user } = get();
        return user ? user.role >= UserRole.Premium : false;
      },

      isAdmin: () => {
        const { user } = get();
        return user ? user.role === UserRole.Admin : false;
      },

      isFree: () => {
        const { user } = get();
        return user ? user.role === UserRole.Free : false;
      },
    }),
    {
      name: 'auth-storage',
      partialize: (state) => ({
        user: state.user,
        accessToken: state.accessToken,
        refreshToken: state.refreshToken,
        isAuthenticated: state.isAuthenticated,
      }),
    }
  )
);