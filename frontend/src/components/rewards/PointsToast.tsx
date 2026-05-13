import { useEffect, useState } from 'react';
import { Coins, Trophy, Flame, Gift, CheckCircle, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import type { TransactionType } from '@/types/reward.types';

interface PointsToastProps {
  /** Point amount (positive for earnings, negative for deductions) */
  amount: number;
  /** Transaction type to determine icon and color */
  type?: TransactionType;
  /** Custom message to display */
  message?: string;
  /** Auto-dismiss duration in milliseconds (0 = no auto-dismiss) */
  duration?: number;
  /** Callback when toast is dismissed */
  onDismiss?: () => void;
  /** Show toast immediately */
  show?: boolean;
}

/**
 * PointsToast Component
 * 
 * Notification toast for point awards and deductions
 * Features:
 * - Animated slide-in from top-right
 * - Type-specific icons and colors
 * - Auto-dismiss after duration
 * - Manual dismiss button
 * - Positive/negative styling based on amount
 * - Celebration animation for large awards (≥100 points)
 * 
 * @example
 * ```tsx
 * // Basic usage
 * <PointsToast amount={50} message="Lesson completed!" />
 * 
 * // With type and custom duration
 * <PointsToast 
 *   amount={100} 
 *   type="CourseCompletion"
 *   message="Course finished!"
 *   duration={5000}
 *   onDismiss={() => console.log('dismissed')}
 * />
 * 
 * // Controlled visibility
 * const [show, setShow] = useState(false);
 * <PointsToast 
 *   amount={200} 
 *   show={show}
 *   onDismiss={() => setShow(false)}
 * />
 * ```
 */
export function PointsToast({
  amount,
  type,
  message,
  duration = 4000,
  onDismiss,
  show = true,
}: PointsToastProps) {
  const [isVisible, setIsVisible] = useState(show);
  const [isExiting, setIsExiting] = useState(false);

  const isPositive = amount > 0;
  const isLargeAmount = Math.abs(amount) >= 100;
  const formattedAmount = `${isPositive ? '+' : ''}${amount.toLocaleString()}`;

  // Get icon and color based on transaction type
  const { icon: Icon, color } = getTypeStyle(type, isPositive);

  // Auto-dismiss after duration
  useEffect(() => {
    if (!show) {
      setIsVisible(false);
      return;
    }

    setIsVisible(true);
    setIsExiting(false);

    if (duration > 0) {
      const timer = setTimeout(() => {
        handleDismiss();
      }, duration);

      return () => clearTimeout(timer);
    }
  }, [show, duration]);

  const handleDismiss = () => {
    setIsExiting(true);
    setTimeout(() => {
      setIsVisible(false);
      onDismiss?.();
    }, 300); // Match exit animation duration
  };

  if (!isVisible) return null;

  return (
    <div
      className={cn(
        'fixed top-4 right-4 z-50 max-w-md',
        'animate-in slide-in-from-top-2 fade-in duration-300',
        isExiting && 'animate-out slide-out-to-right-2 fade-out duration-300'
      )}
      role="alert"
      aria-live="polite"
    >
      <div
        className={cn(
          'relative overflow-hidden rounded-lg shadow-lg border',
          'backdrop-blur-sm',
          isPositive
            ? 'bg-green-50/95 border-green-200 dark:bg-green-950/95 dark:border-green-800'
            : 'bg-red-50/95 border-red-200 dark:bg-red-950/95 dark:border-red-800'
        )}
      >
        {/* Celebration animation for large amounts */}
        {isLargeAmount && isPositive && (
          <div className="absolute inset-0 overflow-hidden pointer-events-none">
            <div className="absolute inset-0 bg-gradient-to-r from-amber-500/20 via-transparent to-amber-500/20 animate-shimmer" />
          </div>
        )}

        <div className="relative p-4 flex items-start gap-3">
          {/* Icon */}
          <div
            className={cn(
              'flex-shrink-0 w-10 h-10 rounded-full flex items-center justify-center',
              isPositive
                ? 'bg-green-500/20 dark:bg-green-500/30'
                : 'bg-red-500/20 dark:bg-red-500/30',
              isLargeAmount && isPositive && 'animate-pulse'
            )}
          >
            <Icon className={cn('w-5 h-5', color)} />
          </div>

          {/* Content */}
          <div className="flex-1 min-w-0">
            <div className="flex items-center gap-2 mb-1">
              <span
                className={cn(
                  'text-2xl font-bold',
                  isPositive
                    ? 'text-green-700 dark:text-green-400'
                    : 'text-red-700 dark:text-red-400'
                )}
              >
                {formattedAmount}
              </span>
              <span
                className={cn(
                  'text-sm font-medium',
                  isPositive
                    ? 'text-green-600 dark:text-green-500'
                    : 'text-red-600 dark:text-red-500'
                )}
              >
                points
              </span>
            </div>
            {message && (
              <p
                className={cn(
                  'text-sm',
                  isPositive
                    ? 'text-green-700 dark:text-green-300'
                    : 'text-red-700 dark:text-red-300'
                )}
              >
                {message}
              </p>
            )}
          </div>

          {/* Dismiss button */}
          <button
            onClick={handleDismiss}
            className={cn(
              'flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center',
              'transition-colors',
              isPositive
                ? 'hover:bg-green-200 dark:hover:bg-green-900 text-green-700 dark:text-green-400'
                : 'hover:bg-red-200 dark:hover:bg-red-900 text-red-700 dark:text-red-400'
            )}
            aria-label="Dismiss notification"
          >
            <X className="w-4 h-4" />
          </button>
        </div>

        {/* Progress bar for auto-dismiss */}
        {duration > 0 && (
          <div
            className={cn(
              'h-1',
              isPositive
                ? 'bg-green-500/30 dark:bg-green-500/20'
                : 'bg-red-500/30 dark:bg-red-500/20'
            )}
          >
            <div
              className={cn(
                'h-full',
                isPositive
                  ? 'bg-green-500 dark:bg-green-400'
                  : 'bg-red-500 dark:bg-red-400'
              )}
              style={{
                animation: `shrink ${duration}ms linear forwards`,
              }}
            />
          </div>
        )}
      </div>
    </div>
  );
}

/**
 * Helper function to get icon and color based on transaction type
 */
function getTypeStyle(type: TransactionType | undefined, isPositive: boolean) {
  if (!type) {
    return {
      icon: isPositive ? Coins : X,
      color: isPositive ? 'text-green-600 dark:text-green-400' : 'text-red-600 dark:text-red-400',
    };
  }

  const styles: Record<TransactionType, { icon: typeof Coins; color: string }> = {
    LessonCompletion: {
      icon: CheckCircle,
      color: 'text-green-600 dark:text-green-400',
    },
    TaskApproval: {
      icon: CheckCircle,
      color: 'text-blue-600 dark:text-blue-400',
    },
    CourseCompletion: {
      icon: Trophy,
      color: 'text-amber-600 dark:text-amber-400',
    },
    DailyStreak: {
      icon: Flame,
      color: 'text-orange-600 dark:text-orange-400',
    },
    ReferralBonus: {
      icon: Gift,
      color: 'text-purple-600 dark:text-purple-400',
    },
    AchievementBonus: {
      icon: Trophy,
      color: 'text-amber-600 dark:text-amber-400',
    },
    AdminBonus: {
      icon: Gift,
      color: 'text-green-600 dark:text-green-400',
    },
    AdminPenalty: {
      icon: X,
      color: 'text-red-600 dark:text-red-400',
    },
    Redemption: {
      icon: Coins,
      color: 'text-red-600 dark:text-red-400',
    },
  };

  return styles[type] || styles.LessonCompletion;
}

// Add keyframe animation for progress bar
if (typeof document !== 'undefined') {
  const style = document.createElement('style');
  style.textContent = `
    @keyframes shrink {
      from {
        width: 100%;
      }
      to {
        width: 0%;
      }
    }
    
    @keyframes shimmer {
      0% {
        transform: translateX(-100%);
      }
      100% {
        transform: translateX(100%);
      }
    }
  `;
  document.head.appendChild(style);
}
