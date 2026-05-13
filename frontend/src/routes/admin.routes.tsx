import { lazy } from 'react';
import type { RouteObject } from 'react-router-dom';
import AdminLayout from '@/layouts/AdminLayout';

// Lazy-loaded admin pages for code splitting
const AdminDashboard = lazy(() => import('@/pages/admin/AdminDashboard'));
const UserManagement = lazy(() => import('@/pages/admin/UserManagement'));
const TaskReview = lazy(() => import('@/pages/admin/TaskReview'));
const CourseManagement = lazy(() => import('@/pages/admin/CourseManagement'));
const DiscountCodes = lazy(() => import('@/pages/admin/DiscountCodes'));
const Analytics = lazy(() => import('@/pages/admin/Analytics'));
const AuditLog = lazy(() => import('@/pages/admin/AuditLog'));

/**
 * Admin route configuration
 * All routes are protected by AdminLayout role guard
 * Pages are lazy-loaded for better performance
 */
export const adminRoutes: RouteObject = {
  path: '/admin',
  element: <AdminLayout />,
  children: [
    {
      index: true,
      element: <AdminDashboard />
    },
    {
      path: 'users',
      element: <UserManagement />
    },
    {
      path: 'tasks',
      element: <TaskReview />
    },
    {
      path: 'courses',
      element: <CourseManagement />
    },
    {
      path: 'rewards',
      element: <DiscountCodes />
    },
    {
      path: 'analytics',
      element: <Analytics />
    },
    {
      path: 'audit',
      element: <AuditLog />
    }
  ]
};

export default adminRoutes;
