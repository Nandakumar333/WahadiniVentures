import { NavLink } from 'react-router-dom';
import {
  LayoutDashboard,
  Users,
  ClipboardCheck,
  BookOpen,
  Gift,
  BarChart3,
  FileText,
  LogOut
} from 'lucide-react';
import { useAuth } from '@/hooks/auth/useAuth';

/**
 * AdminSidebar - Navigation sidebar for admin dashboard
 * Displays navigation links to all admin pages with icons
 */
const AdminSidebar = () => {
  const { logout } = useAuth();

  const navigationItems = [
    {
      name: 'Dashboard',
      path: '/admin',
      icon: LayoutDashboard,
      description: 'Platform overview and metrics'
    },
    {
      name: 'Users',
      path: '/admin/users',
      icon: Users,
      description: 'User management'
    },
    {
      name: 'Tasks',
      path: '/admin/tasks',
      icon: ClipboardCheck,
      description: 'Task review and approval'
    },
    {
      name: 'Courses',
      path: '/admin/courses',
      icon: BookOpen,
      description: 'Course management'
    },
    {
      name: 'Rewards',
      path: '/admin/rewards',
      icon: Gift,
      description: 'Discount codes and points'
    },
    {
      name: 'Analytics',
      path: '/admin/analytics',
      icon: BarChart3,
      description: 'Platform analytics'
    },
    {
      name: 'Audit Log',
      path: '/admin/audit',
      icon: FileText,
      description: 'Administrative actions log'
    }
  ];

  const handleLogout = () => {
    logout();
  };

  return (
    <aside className="w-64 bg-gray-900 text-white flex flex-col">
      {/* Logo */}
      <div className="px-6 py-6 border-b border-gray-700">
        <h2 className="text-xl font-bold text-white">
          Wahadini<span className="text-purple-400">Admin</span>
        </h2>
        <p className="text-xs text-gray-400 mt-1">Management Panel</p>
      </div>

      {/* Navigation Links */}
      <nav className="flex-1 px-4 py-6 space-y-2 overflow-y-auto">
        {navigationItems.map((item) => {
          const Icon = item.icon;
          return (
            <NavLink
              key={item.path}
              to={item.path}
              end={item.path === '/admin'}
              className={({ isActive }) =>
                `flex items-center gap-3 px-4 py-3 rounded-lg transition-colors group ${
                  isActive
                    ? 'bg-purple-600 text-white'
                    : 'text-gray-300 hover:bg-gray-800 hover:text-white'
                }`
              }
              title={item.description}
            >
              <Icon className="w-5 h-5" />
              <span className="font-medium">{item.name}</span>
            </NavLink>
          );
        })}
      </nav>

      {/* Logout Button */}
      <div className="px-4 py-4 border-t border-gray-700">
        <button
          onClick={handleLogout}
          className="flex items-center gap-3 w-full px-4 py-3 rounded-lg text-gray-300 hover:bg-red-600 hover:text-white transition-colors"
        >
          <LogOut className="w-5 h-5" />
          <span className="font-medium">Logout</span>
        </button>
      </div>
    </aside>
  );
};

export default AdminSidebar;
