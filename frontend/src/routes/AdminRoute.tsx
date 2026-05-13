import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/auth/useAuth'

interface AdminRouteProps {
  children: React.ReactNode
  redirectTo?: string
}

/**
 * Admin route component that requires Admin role
 * Redirects to unauthorized page if user doesn't have admin access
 * Shows loading spinner while checking authentication
 */
export const AdminRoute: React.FC<AdminRouteProps> = ({
  children,
  redirectTo = '/unauthorized'
}) => {
  const { isAuthenticated, isAdmin, isLoading } = useAuth()
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
    return <Navigate to="/auth/login" state={{ from: location }} replace />
  }

  // Redirect to unauthorized page if not admin
  if (!isAdmin) {
    return <Navigate to={redirectTo} state={{ from: location }} replace />
  }

  return <>{children}</>
}

export default AdminRoute
