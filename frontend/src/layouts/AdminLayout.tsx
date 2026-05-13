import { Navigate, Outlet } from 'react-router-dom';
import { useAuth } from '@/hooks/auth/useAuth';
import AdminSidebar from '@/components/admin/AdminSidebar';
import { UserRole } from '@/types/api';

/**
 * AdminLayout - Layout wrapper for admin dashboard pages
 * Provides sidebar navigation and role-based access control
 * Only accessible to users with Admin or SuperAdmin roles
 */
const AdminLayout = () => {
  const { user, isAuthenticated } = useAuth();

  // Redirect to login if not authenticated
  if (!isAuthenticated) {
    return <Navigate to="/auth/login" replace />;
  }

  // Role guard - only Admin (2) and SuperAdmin (3) can access
  const isAdmin = user?.role === UserRole.Admin || user?.role === UserRole.SuperAdmin;
  
  if (!isAdmin) {
    return <Navigate to="/dashboard" replace />;
  }

  return (
    <div className="flex min-h-screen bg-gray-50">
      {/* Sidebar Navigation */}
      <AdminSidebar />

      {/* Main Content Area */}
      <div className="flex-1 flex flex-col">
        {/* Header */}
        <header className="bg-white shadow-sm border-b border-gray-200">
          <div className="px-6 py-4">
            <div className="flex items-center justify-between">
              <h1 className="text-2xl font-bold text-gray-900">
                Admin Dashboard
              </h1>
              <div className="flex items-center gap-4">
                <span className="text-sm text-gray-600">
                  {user?.firstName} {user?.lastName}
                </span>
                <span className="px-3 py-1 text-xs font-medium rounded-full bg-purple-100 text-purple-800">
                  {user?.role}
                </span>
              </div>
            </div>
          </div>
        </header>

        {/* Page Content */}
        <main className="flex-1 p-6 overflow-auto">
          <Outlet />
        </main>

        {/* Footer */}
        <footer className="bg-white border-t border-gray-200 px-6 py-4">
          <p className="text-sm text-gray-500 text-center">
            WahadiniCryptoQuest Admin Panel © {new Date().getFullYear()}
          </p>
        </footer>
      </div>
    </div>
  );
};

export default AdminLayout;
