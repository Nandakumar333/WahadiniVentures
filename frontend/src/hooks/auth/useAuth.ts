import { useAuthStore } from '@/store/authStore'

/**
 * Custom hook for authentication operations
 * Provides a clean interface to the auth store
 */
export const useAuth = () => {
  const {
    user,
    accessToken,
    refreshToken,
    isAuthenticated,
    isLoading,
    error,
    setUser,
    setTokens,
    login,
    register,
    verifyEmail,
    logout,
    setLoading,
    setError,
    hasRole,
    isPremium,
    isAdmin,
    isFree,
  } = useAuthStore()

  return {
    // State
    user,
    accessToken,
    refreshToken,
    isAuthenticated,
    isLoading,
    error,
    
    // Actions
    login,
    register,
    verifyEmail,
    logout,
    setUser,
    setTokens,
    setLoading,
    setError,
    
    // Role checking
    hasRole,
    isPremium,
    isAdmin,
    isFree,
    
    // Computed values
    isLoggedIn: isAuthenticated && !!user,
    hasToken: !!accessToken,
  }
}

export default useAuth