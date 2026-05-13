import { createBrowserRouter, Navigate } from 'react-router-dom'
import { lazy, Suspense } from 'react'

// Route Components
import ProtectedRoute from './ProtectedRoute'
import PublicRoute from './PublicRoute'
import AdminRoute from './AdminRoute'
import { LazyWrapper } from '@/components/common/LazyWrapper'
import { Layout } from '@/components/layout/Layout'
import { MainLayout } from '@/components/layout/MainLayout'
import AdminLayout from '@/layouts/AdminLayout'

// Lazy load pages for better performance
const HomePage = lazy(() => import('@/pages/HomePage'))
const LoginPage = lazy(() => import('@/pages/LoginPage'))
const RegisterPage = lazy(() => import('@/pages/auth/RegisterPage'))
const EmailVerificationPage = lazy(() => import('@/pages/auth/EmailVerificationPage').then(module => ({ default: module.EmailVerificationPage })))
const ForgotPasswordPage = lazy(() => import('@/pages/auth/ForgotPasswordPage').then(module => ({ default: module.ForgotPasswordPage })))
const ResetPasswordPage = lazy(() => import('@/pages/auth/ResetPasswordPage').then(module => ({ default: module.ResetPasswordPage })))
const DashboardPage = lazy(() => import('@/pages/DashboardPage'))
const MyCoursesPage = lazy(() => import('@/pages/MyCoursesPage').then(module => ({ default: module.MyCoursesPage })))
const CoursesPage = lazy(() => import('@/pages/courses/CoursesPage').then(module => ({ default: module.CoursesPage })))
const CourseDetailPage = lazy(() => import('@/pages/courses/CourseDetailPage').then(module => ({ default: module.CourseDetailPage })))
const LessonPage = lazy(() => import('@/pages/lesson/LessonPage')) // Code splitting for lesson page (T306)
const UnauthorizedPage = lazy(() => import('@/pages/UnauthorizedPage').then(module => ({ default: module.UnauthorizedPage })))

// Lazy load admin pages for code splitting (T164, 009-admin-dashboard T020)
const AdminDashboard = lazy(() => import('@/pages/admin/AdminDashboard'))
const AdminCoursesPage = lazy(() => import('@/pages/admin/courses/AdminCoursesPage'))
const AdminTasksPage = lazy(() => import('@/pages/admin/AdminTasksPage').then(module => ({ default: module.AdminTasksPage })))
const UserManagement = lazy(() => import('@/pages/admin/UserManagement'))
const TaskReview = lazy(() => import('@/pages/admin/TaskReview'))
const CourseManagement = lazy(() => import('@/pages/admin/CourseManagement'))
const DiscountCodes = lazy(() => import('@/pages/admin/DiscountCodes'))
const Analytics = lazy(() => import('@/pages/admin/Analytics'))
const AuditLog = lazy(() => import('@/pages/admin/AuditLog'))

const MySubmissionsPage = lazy(() => import('@/pages/MySubmissionsPage').then(module => ({ default: module.MySubmissionsPage })))

// Lazy load rewards pages (T074)
const LeaderboardPage = lazy(() => import('@/pages/rewards/Leaderboard').then(module => ({ default: module.LeaderboardPage })))
const RewardsPage = lazy(() => import('@/pages/rewards/RewardsPage').then(module => ({ default: module.RewardsPage })))

// Lazy load discount pages (T025, T053)
const AvailableDiscountsPage = lazy(() => import('@/pages/discount/AvailableDiscountsPage').then(module => ({ default: module.AvailableDiscountsPage })))
const MyDiscountsPage = lazy(() => import('@/pages/discount/MyDiscountsPage').then(module => ({ default: module.MyDiscountsPage })))

// Lazy load admin discount page (T078, T092)
const AdminDiscountsPage = lazy(() => import('@/pages/admin/AdminDiscountsPage').then(module => ({ default: module.AdminDiscountsPage })))
const DiscountAnalyticsPage = lazy(() => import('@/pages/admin/DiscountAnalyticsPage').then(module => ({ default: module.DiscountAnalyticsPage })))

// Lazy load admin currency management page (Phase 7)
const AdminCurrencyManagementPage = lazy(() => import('@/pages/admin/AdminCurrencyManagementPage').then(module => ({ default: module.AdminCurrencyManagementPage })))

// Lazy load subscription pages (008-stripe-subscription, Phase 6)
const PricingPage = lazy(() => import('@/pages/subscription/PricingPage').then(module => ({ default: module.PricingPage })))
const CheckoutSuccessPage = lazy(() => import('@/pages/subscription/CheckoutSuccessPage').then(module => ({ default: module.CheckoutSuccessPage })))
const CheckoutCancelPage = lazy(() => import('@/pages/subscription/CheckoutCancelPage').then(module => ({ default: module.CheckoutCancelPage })))
const ManageSubscriptionPage = lazy(() => import('@/pages/subscription/ManageSubscriptionPage').then(module => ({ default: module.ManageSubscriptionPage })))

/**
 * Main application router configuration
 * Supports protected routes and lazy loading
 */
export const router = createBrowserRouter([
  {
    path: '/',
    element: <Layout />,
    children: [
      {
        index: true,
        element: <LazyWrapper><HomePage /></LazyWrapper>
      },
      // Public Auth Routes (no layout)
      {
        path: 'login',
        element: (
          <PublicRoute>
            <LazyWrapper><LoginPage /></LazyWrapper>
          </PublicRoute>
        )
      },
      {
        path: 'register',
        element: (
          <PublicRoute>
            <LazyWrapper><RegisterPage /></LazyWrapper>
          </PublicRoute>
        )
      },
      {
        path: 'auth',
        children: [
          {
            path: 'login',
            element: (
              <PublicRoute>
                <LazyWrapper><LoginPage /></LazyWrapper>
              </PublicRoute>
            )
          },
          {
            path: 'register',
            element: (
              <PublicRoute>
                <LazyWrapper><RegisterPage /></LazyWrapper>
              </PublicRoute>
            )
          },
          {
            path: 'verify-email',
            element: (
              <PublicRoute>
                <LazyWrapper><EmailVerificationPage /></LazyWrapper>
              </PublicRoute>
            )
          },
          {
            path: 'forgot-password',
            element: (
              <PublicRoute>
                <LazyWrapper><ForgotPasswordPage /></LazyWrapper>
              </PublicRoute>
            )
          },
          {
            path: 'reset-password',
            element: (
              <PublicRoute>
                <LazyWrapper><ResetPasswordPage /></LazyWrapper>
              </PublicRoute>
            )
          }
        ]
      }
    ]
  },
  // Protected Routes with MainLayout (Header + Sidebar)
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: 'dashboard',
        element: <LazyWrapper><DashboardPage /></LazyWrapper>
      },
      {
        path: 'my-courses',
        element: <LazyWrapper><MyCoursesPage /></LazyWrapper>
      },
      {
        path: 'my-submissions',
        element: <LazyWrapper><MySubmissionsPage /></LazyWrapper>
      },
      {
        path: 'courses',
        element: <LazyWrapper><CoursesPage /></LazyWrapper>
      },
      {
        path: 'courses/:courseId',
        element: <LazyWrapper><CourseDetailPage /></LazyWrapper>
      },
      {
        path: 'lessons/:lessonId',
        element: <LazyWrapper><LessonPage /></LazyWrapper>
      },
      {
        path: 'leaderboard',
        element: <LazyWrapper><LeaderboardPage /></LazyWrapper>
      },
      {
        path: 'rewards',
        children: [
          {
            index: true,
            element: <LazyWrapper><RewardsPage /></LazyWrapper>
          },
          {
            path: 'leaderboard',
            element: <LazyWrapper><LeaderboardPage /></LazyWrapper>
          }
        ]
      },
      {
        path: 'discounts',
        children: [
          {
            index: true,
            element: <LazyWrapper><AvailableDiscountsPage /></LazyWrapper>
          },
          {
            path: 'my-redemptions',
            element: <LazyWrapper><MyDiscountsPage /></LazyWrapper>
          }
        ]
      },
      {
        path: 'pricing',
        element: <LazyWrapper><PricingPage /></LazyWrapper>
      },
      {
        path: 'subscription',
        children: [
          {
            path: 'success',
            element: <LazyWrapper><CheckoutSuccessPage /></LazyWrapper>
          },
          {
            path: 'cancel',
            element: <LazyWrapper><CheckoutCancelPage /></LazyWrapper>
          },
          {
            path: 'manage',
            element: <LazyWrapper><ManageSubscriptionPage /></LazyWrapper>
          }
        ]
      },
      {
        path: 'unauthorized',
        element: <LazyWrapper><UnauthorizedPage /></LazyWrapper>
      },
      {
        path: 'admin',
        children: [
          {
            path: 'courses',
            element: (
              <AdminRoute>
                <LazyWrapper><AdminCoursesPage /></LazyWrapper>
              </AdminRoute>
            )
          },
          {
            path: 'tasks',
            element: (
              <AdminRoute>
                <LazyWrapper><AdminTasksPage /></LazyWrapper>
              </AdminRoute>
            )
          },
          {
            path: 'discounts',
            element: (
              <AdminRoute>
                <LazyWrapper><AdminDiscountsPage /></LazyWrapper>
              </AdminRoute>
            )
          },
          {
            path: 'discounts/analytics',
            element: (
              <AdminRoute>
                <LazyWrapper><DiscountAnalyticsPage /></LazyWrapper>
              </AdminRoute>
            )
          },
          {
            path: 'currency-pricing',
            element: (
              <AdminRoute>
                <LazyWrapper><AdminCurrencyManagementPage /></LazyWrapper>
              </AdminRoute>
            )
          }
        ]
      }
    ]
  },
  // Admin Dashboard Routes with AdminLayout (009-admin-dashboard T021)
  {
    path: '/admin',
    element: (
      <Suspense fallback={<div className="flex items-center justify-center min-h-screen"><div className="animate-spin rounded-full h-12 w-12 border-b-2 border-purple-600"></div></div>}>
        <AdminLayout />
      </Suspense>
    ),
    children: [
      {
        index: true,
        element: <LazyWrapper><AdminDashboard /></LazyWrapper>
      },
      {
        path: 'users',
        element: <LazyWrapper><UserManagement /></LazyWrapper>
      },
      {
        path: 'tasks-review',
        element: <LazyWrapper><TaskReview /></LazyWrapper>
      },
      {
        path: 'courses-mgmt',
        element: <LazyWrapper><CourseManagement /></LazyWrapper>
      },
      {
        path: 'rewards',
        element: <LazyWrapper><DiscountCodes /></LazyWrapper>
      },
      {
        path: 'analytics',
        element: <LazyWrapper><Analytics /></LazyWrapper>
      },
      {
        path: 'audit',
        element: <LazyWrapper><AuditLog /></LazyWrapper>
      }
    ]
  },
  // Protected Routes with MainLayout (Header + Sidebar)
  {
    path: '/',
    element: (
      <ProtectedRoute>
        <MainLayout />
      </ProtectedRoute>
    ),
    children: [
      {
        path: 'dashboard',
        element: <LazyWrapper><DashboardPage /></LazyWrapper>
      },
      {
        path: 'my-courses',
        element: <LazyWrapper><MyCoursesPage /></LazyWrapper>
      },
      {
        path: 'my-submissions',
        element: <LazyWrapper><MySubmissionsPage /></LazyWrapper>
      },
      {
        path: 'courses',
        element: <LazyWrapper><CoursesPage /></LazyWrapper>
      },
      {
        path: 'courses/:courseId',
        element: <LazyWrapper><CourseDetailPage /></LazyWrapper>
      },
      {
        path: 'lessons/:lessonId',
        element: <LazyWrapper><LessonPage /></LazyWrapper>
      },
      {
        path: 'leaderboard',
        element: <LazyWrapper><LeaderboardPage /></LazyWrapper>
      },
      {
        path: 'rewards',
        children: [
          {
            index: true,
            element: <LazyWrapper><RewardsPage /></LazyWrapper>
          },
          {
            path: 'leaderboard',
            element: <LazyWrapper><LeaderboardPage /></LazyWrapper>
          }
        ]
      },
      {
        path: 'discounts',
        children: [
          {
            index: true,
            element: <LazyWrapper><AvailableDiscountsPage /></LazyWrapper>
          },
          {
            path: 'my-redemptions',
            element: <LazyWrapper><MyDiscountsPage /></LazyWrapper>
          }
        ]
      },
      {
        path: 'pricing',
        element: <LazyWrapper><PricingPage /></LazyWrapper>
      },
      {
        path: 'subscription',
        children: [
          {
            path: 'success',
            element: <LazyWrapper><CheckoutSuccessPage /></LazyWrapper>
          },
          {
            path: 'cancel',
            element: <LazyWrapper><CheckoutCancelPage /></LazyWrapper>
          },
          {
            path: 'manage',
            element: <LazyWrapper><ManageSubscriptionPage /></LazyWrapper>
          }
        ]
      },
      {
        path: 'unauthorized',
        element: <LazyWrapper><UnauthorizedPage /></LazyWrapper>
      }
    ]
  },
  {
    path: '*',
    element: <Navigate to="/" replace />
  }
])

export default router