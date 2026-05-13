import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/auth/useAuth'

interface ProtectedRouteProps {
  children: React.ReactNode
  redirectTo?: string
  requiredRoles?: string[]
}

/**
 * Protected route component that requires authentication
 * Redirects to login page if user is not authenticated
 * Optionally checks for required roles
 */
export const ProtectedRoute: React.FC<ProtectedRouteProps> = ({
  children,
  redirectTo = '/auth/login',
  requiredRoles = []
}) => {
  const { isAuthenticated, isLoading, user } = useAuth()
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

  // Check for required roles if specified
  if (requiredRoles.length > 0 && user) {
    // Note: This assumes user object has a roles property
    // Adjust based on your actual user structure
    // Basic role check (can be expanded)
    const userRoles = user?.roles || []
    const hasRequiredRole = requiredRoles.some(role => 
      userRoles.includes(role)
    )
    
    if (!hasRequiredRole) {
      // Redirect to unauthorized page or dashboard
      return <Navigate to="/unauthorized" replace />
    }
  }

  return <>{children}</>
}

export default ProtectedRoute