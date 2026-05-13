import React from 'react';
import { Check, Clock } from 'lucide-react';

export interface LessonProgressBadgeProps {
  completionPercentage: number;
  isCompleted: boolean;
}

export const LessonProgressBadge: React.FC<LessonProgressBadgeProps> = ({
  completionPercentage,
  isCompleted,
}) => {
  const percentage = Math.min(100, Math.max(0, completionPercentage));
  const isNotStarted = percentage === 0 && !isCompleted;

  // Determine badge style based on status
  const getBadgeStyles = () => {
    if (isCompleted) {
      return {
        container: 'bg-green-100 border-green-500 text-green-800',
        icon: 'text-green-600',
        text: 'text-green-900 font-semibold',
      };
    }
    if (isNotStarted) {
      return {
        container: 'bg-gray-100 border-gray-300 text-gray-600',
        icon: 'text-gray-500',
        text: 'text-gray-700',
      };
    }
    return {
      container: 'bg-blue-100 border-blue-500 text-blue-800',
      icon: 'text-blue-600',
      text: 'text-blue-900',
    };
  };

  const styles = getBadgeStyles();

  // Calculate circular progress for SVG
  const radius = 16;
  const circumference = 2 * Math.PI * radius;
  const strokeDashoffset = circumference - (percentage / 100) * circumference;

  return (
    <div
      className={`inline-flex items-center gap-2 px-3 py-1.5 rounded-full border-2 transition-all duration-300 ${styles.container}`}
      role="status"
      aria-label={
        isCompleted
          ? 'Lesson completed'
          : isNotStarted
          ? 'Lesson not started'
          : `Lesson ${percentage}% complete`
      }
    >
      {/* Circular Progress Indicator */}
      {!isCompleted && !isNotStarted && (
        <div className="relative w-8 h-8">
          {/* Background circle */}
          <svg className="transform -rotate-90 w-8 h-8" viewBox="0 0 40 40">
            <circle
              cx="20"
              cy="20"
              r={radius}
              stroke="currentColor"
              strokeWidth="3"
              fill="none"
              className="text-blue-200"
            />
            {/* Progress circle */}
            <circle
              cx="20"
              cy="20"
              r={radius}
              stroke="currentColor"
              strokeWidth="3"
              fill="none"
              strokeDasharray={circumference}
              strokeDashoffset={strokeDashoffset}
              className="text-blue-600 transition-all duration-500"
              strokeLinecap="round"
            />
          </svg>
          {/* Percentage text inside circle */}
          <div className="absolute inset-0 flex items-center justify-center">
            <span className="text-[10px] font-bold text-blue-900">
              {Math.round(percentage)}
            </span>
          </div>
        </div>
      )}

      {/* Completed Icon */}
      {isCompleted && (
        <div className="flex items-center justify-center w-5 h-5 bg-green-600 rounded-full">
          <Check className={`w-3.5 h-3.5 text-white`} strokeWidth={3} />
        </div>
      )}

      {/* Not Started Icon */}
      {isNotStarted && <Clock className={`w-4 h-4 ${styles.icon}`} />}

      {/* Status Text */}
      <span className={`text-sm ${styles.text}`}>
        {isCompleted && 'Completed'}
        {isNotStarted && 'Not Started'}
        {!isCompleted && !isNotStarted && `${Math.round(percentage)}%`}
      </span>
    </div>
  );
};

export default LessonProgressBadge;
