import { Navigate, useLocation } from 'react-router-dom'
import { useAuth } from '@/hooks/auth/useAuth'

interface PremiumRouteProps {
  children: React.ReactNode
  redirectTo?: string
  showUpgradePrompt?: boolean
}

/**
 * Premium route component that requires Premium or Admin role
 * Redirects to upgrade page if user doesn't have premium access
 * Shows loading spinner while checking authentication
 */
export const PremiumRoute: React.FC<PremiumRouteProps> = ({
  children,
  redirectTo = '/upgrade',
  showUpgradePrompt = true
}) => {
  const { isAuthenticated, isPremium, isAdmin, isLoading } = useAuth()
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

  // Check if user has premium or admin access
  const hasPremiumAccess = isPremium || isAdmin

  // Redirect to upgrade page if no premium access
  if (!hasPremiumAccess) {
    return <Navigate to={redirectTo} state={{ from: location, showUpgradePrompt }} replace />
  }

  return <>{children}</>
}

export default PremiumRoute
