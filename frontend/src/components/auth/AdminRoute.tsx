import React from 'react';
import { Navigate } from 'react-router-dom';
import { useAuthStore } from '@/store/authStore';

interface AdminRouteProps {
  children: React.ReactNode;
}

/**
 * AdminRoute Component
 * 
 * Protects admin-only routes by checking user role.
 * Redirects non-admin users to 403 Forbidden page.
 * 
 * Usage:
 * ```tsx
 * <AdminRoute>
 *   <AdminDashboard />
 * </AdminRoute>
 * ```
 * 
 * Features:
 * - Role-based access control
 * - Automatic redirection for unauthorized users
 * - Works with React Router 7
 * - Type-safe with TypeScript
 */
export const AdminRoute: React.FC<AdminRouteProps> = ({ children }) => {
  const { isAdmin, isAuthenticated } = useAuthStore();

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/login" replace state={{ from: window.location.pathname }} />;
  }

  // Redirect to 403 if authenticated but not admin
  if (!isAdmin()) {
    return <Navigate to="/403" replace />;
  }

  // Render children for admin users
  return <>{children}</>;
};

export default AdminRoute;
