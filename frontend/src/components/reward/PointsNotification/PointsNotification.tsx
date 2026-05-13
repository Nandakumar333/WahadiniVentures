import { useEffect, useState, useRef } from 'react';
import { Trophy, X, Sparkles } from 'lucide-react';

interface PointsNotificationProps {
  points: number;
  show: boolean;
  onClose: () => void;
  title?: string;
}

export function PointsNotification({ 
  points, 
  show, 
  onClose,
  title = 'Lesson Completed!'
}: PointsNotificationProps) {
  const [isVisible, setIsVisible] = useState(false);
  const [isExiting, setIsExiting] = useState(false);
  const timerRef = useRef<NodeJS.Timeout | null>(null);

  useEffect(() => {
    if (show) {
      // Trigger entrance animation
      setIsVisible(true);
      setIsExiting(false);

      // Auto-dismiss after 5 seconds
      timerRef.current = setTimeout(() => {
        handleClose();
      }, 5000);

      return () => {
        if (timerRef.current) {
          clearTimeout(timerRef.current);
          timerRef.current = null;
        }
      };
    }
  }, [show]);

  const handleClose = () => {
    // Clear the auto-dismiss timer if it exists
    if (timerRef.current) {
      clearTimeout(timerRef.current);
      timerRef.current = null;
    }

    setIsExiting(true);
    // Wait for exit animation before hiding
    setTimeout(() => {
      setIsVisible(false);
      onClose();
    }, 300);
  };

  if (!show && !isVisible) return null;

  return (
    <div
      className={`fixed top-4 right-4 z-50 transition-all duration-300 ease-out ${
        isExiting 
          ? 'opacity-0 translate-x-full scale-95' 
          : isVisible 
          ? 'opacity-100 translate-x-0 scale-100 animate-in slide-in-from-right' 
          : 'opacity-0 translate-x-full scale-95'
      }`}
      role="alert"
      aria-live="assertive"
      aria-atomic="true"
    >
      <div className="relative bg-gradient-to-r from-green-500 via-green-600 to-emerald-600 text-white rounded-2xl shadow-2xl p-6 min-w-[320px] max-w-md overflow-hidden">
        {/* Background decoration */}
        <div className="absolute inset-0 bg-[url('data:image/svg+xml;base64,PHN2ZyB3aWR0aD0iMjAwIiBoZWlnaHQ9IjIwMCIgeG1sbnM9Imh0dHA6Ly93d3cudzMub3JnLzIwMDAvc3ZnIj48ZGVmcz48cGF0dGVybiBpZD0iZ3JpZCIgd2lkdGg9IjQwIiBoZWlnaHQ9IjQwIiBwYXR0ZXJuVW5pdHM9InVzZXJTcGFjZU9uVXNlIj48cGF0aCBkPSJNIDQwIDAgTCAwIDAgMCA0MCIgZmlsbD0ibm9uZSIgc3Ryb2tlPSJyZ2JhKDI1NSwgMjU1LCAyNTUsIDAuMSkiIHN0cm9rZS13aWR0aD0iMSIvPjwvcGF0dGVybj48L2RlZnM+PHJlY3Qgd2lkdGg9IjEwMCUiIGhlaWdodD0iMTAwJSIgZmlsbD0idXJsKCNncmlkKSIvPjwvc3ZnPg==')] opacity-20" />

        {/* Sparkle animations */}
        <div className="absolute top-2 left-2 animate-pulse">
          <Sparkles className="w-4 h-4 text-yellow-300" />
        </div>
        <div className="absolute top-4 right-8 animate-pulse delay-150">
          <Sparkles className="w-3 h-3 text-yellow-200" />
        </div>
        <div className="absolute bottom-2 left-8 animate-pulse delay-300">
          <Sparkles className="w-3 h-3 text-yellow-200" />
        </div>

        {/* Close button */}
        <button
          onClick={handleClose}
          className="absolute top-3 right-3 p-1 rounded-full hover:bg-white/20 transition-colors"
          aria-label="Close notification"
        >
          <X className="w-5 h-5" />
        </button>

        {/* Content */}
        <div className="relative flex items-start gap-4">
          {/* Trophy icon with celebration animation */}
          <div className="flex-shrink-0">
            <div className="relative">
              <div className="absolute inset-0 bg-yellow-300 rounded-full blur-xl opacity-50 animate-pulse" />
              <div className="relative bg-white/20 backdrop-blur-sm p-3 rounded-full animate-bounce">
                <Trophy className="w-8 h-8 text-yellow-300" />
              </div>
            </div>
          </div>

          {/* Text content */}
          <div className="flex-1 pt-1">
            <h3 className="text-xl font-bold mb-1">{title}</h3>
            <div className="flex items-baseline gap-2">
              <span className="text-3xl font-extrabold text-yellow-300 animate-pulse">
                +{points}
              </span>
              <span className="text-lg font-semibold">
                point{points !== 1 ? 's' : ''} earned!
              </span>
            </div>
            <p className="mt-2 text-sm text-green-100 opacity-90">
              Great job! Keep learning to earn more points.
            </p>
          </div>
        </div>

        {/* Progress bar for auto-dismiss */}
        <div className="absolute bottom-0 left-0 right-0 h-1 bg-white/20">
          <div 
            className="h-full bg-white/40 animate-shrink origin-left"
            style={{ 
              animation: isExiting ? 'none' : 'shrink 5s linear forwards'
            }}
          />
        </div>
      </div>

      <style>{`
        @keyframes shrink {
          from { width: 100%; }
          to { width: 0%; }
        }
        
        .animate-shrink {
          animation: shrink 5s linear forwards;
        }

        @keyframes slide-in-from-right {
          from {
            transform: translateX(100%);
            opacity: 0;
          }
          to {
            transform: translateX(0);
            opacity: 1;
          }
        }

        .animate-in {
          animation: slide-in-from-right 0.3s ease-out;
        }

        .delay-150 {
          animation-delay: 0.15s;
        }

        .delay-300 {
          animation-delay: 0.3s;
        }
      `}</style>
    </div>
  );
}
