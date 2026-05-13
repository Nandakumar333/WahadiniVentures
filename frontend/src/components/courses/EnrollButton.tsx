import React, { useState } from 'react';
import { useEnrollment } from '../../hooks/courses/useEnrollment';
import { useAuthStore } from '../../store/authStore';
import { UserRole } from '../../types/api';
import type { CourseDetail } from '../../types/course.types';
import { Loader2, Lock, CheckCircle, ArrowRight } from 'lucide-react';
import { Button } from '@/components/ui/button';

interface EnrollButtonProps {
  course: CourseDetail;
  onUpgradeClick?: () => void;
}

/**
 * Enrollment button component with premium gate
 * Shows appropriate state based on enrollment status and user role
 */
export const EnrollButton: React.FC<EnrollButtonProps> = ({
  course,
  onUpgradeClick,
}) => {
  const { user } = useAuthStore();
  const { mutate: enrollInCourse, isPending } = useEnrollment();
  const [showSuccess, setShowSuccess] = useState(false);

  // Check if user is authenticated
  if (!user) {
    return (
      <Button
        asChild
        className="w-full h-12 text-lg font-semibold bg-blue-600 hover:bg-blue-700 shadow-lg hover:shadow-blue-500/25 transition-all duration-300"
      >
        <a href="/login">
          Sign in to Enroll
          <ArrowRight className="w-5 h-5 ml-2" />
        </a>
      </Button>
    );
  }

  // Already enrolled - show success state
  if (course.isEnrolled) {
    return (
      <div className="flex items-center justify-center w-full h-12 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg text-green-700 dark:text-green-400 font-semibold">
        <CheckCircle className="w-5 h-5 mr-2" />
        Enrolled
      </div>
    );
  }

  // Premium course but free user - show upgrade prompt
  if (course.isPremium && user.role === UserRole.Free) {
    return (
      <Button
        onClick={onUpgradeClick}
        className="w-full h-12 text-lg font-bold bg-gradient-to-r from-yellow-400 to-yellow-600 hover:from-yellow-500 hover:to-yellow-700 text-gray-900 shadow-lg hover:shadow-yellow-500/25 transition-all duration-300 border-none"
      >
        <Lock className="w-5 h-5 mr-2" />
        Upgrade to Enroll
      </Button>
    );
  }

  // Handle enrollment
  const handleEnroll = () => {
    enrollInCourse(
      { courseId: course.id },
      {
        onSuccess: () => {
          setShowSuccess(true);
          setTimeout(() => setShowSuccess(false), 3000);
        },
        onError: (error) => {
          console.error('Enrollment failed:', error);
        },
      }
    );
  };

  // Show temporary success message after enrollment
  if (showSuccess) {
    return (
      <div className="flex items-center justify-center w-full h-12 bg-green-50 dark:bg-green-900/20 border border-green-200 dark:border-green-800 rounded-lg text-green-700 dark:text-green-400 font-semibold animate-in fade-in zoom-in duration-300">
        <CheckCircle className="w-5 h-5 mr-2" />
        Successfully Enrolled!
      </div>
    );
  }

  // Default - show enroll button
  return (
    <Button
      onClick={handleEnroll}
      disabled={isPending}
      className="w-full h-12 text-lg font-semibold bg-gradient-to-r from-blue-600 to-indigo-600 hover:from-blue-700 hover:to-indigo-700 text-white shadow-lg hover:shadow-blue-500/25 transition-all duration-300"
    >
      {isPending ? (
        <>
          <Loader2 className="w-5 h-5 mr-2 animate-spin" />
          Enrolling...
        </>
      ) : (
        <>
          Enroll Now
          <ArrowRight className="w-5 h-5 ml-2" />
        </>
      )}
    </Button>
  );
};
