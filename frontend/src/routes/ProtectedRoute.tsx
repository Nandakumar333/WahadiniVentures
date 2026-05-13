import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/auth/useAuth'

interface ProtectedRouteProps {
  children: React.ReactNode
  redirectTo?: string
}

/**
 * Protected route component that requires authentication
 * Redirects to login page if user is not authenticated
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  redirectTo = '/auth/login'
}) => {
  const { isAuthenticated, isLoading } = useAuth()
  const location = useLocation()

  // Show loading spinner while checking authentication
  if (isLoading) {
    return (
      <div className="min-h-screen flex items-center justify-center">
        <div className="animate-spin rounded-full h-32 w-32 border-b-2 border-primary"></div>
      </div>
    )
  }

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />
  }

  return <>{children}</>
}

export default ProtectedRoute