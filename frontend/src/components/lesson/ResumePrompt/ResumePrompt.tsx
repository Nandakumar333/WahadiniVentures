import { Play, RotateCcw } from 'lucide-react';
import { formatSeconds } from '@/utils/formatters/time.formatter';

interface ResumePromptProps {
  resumePosition: number;
  videoDuration: number;
  onResume: () => void;
  onStartFromBeginning: () => void;
}

export function ResumePrompt({
  resumePosition,
  videoDuration,
  onResume,
  onStartFromBeginning,
}: ResumePromptProps) {
  // If resume position is invalid or too small, automatically start from beginning
  const isValidPosition = resumePosition > 5 && resumePosition <= videoDuration;
  
  if (!isValidPosition) {
    // Auto-start from beginning if position is invalid
    setTimeout(() => onStartFromBeginning(), 0);
    return null;
  }

  return (
    <div 
      className="absolute inset-0 z-20 flex items-center justify-center bg-black/80 backdrop-blur-sm"
      role="dialog"
      aria-modal="true"
      aria-labelledby="resume-prompt-title"
    >
      <div className="bg-white dark:bg-gray-800 rounded-2xl shadow-2xl p-8 max-w-md w-full mx-4 animate-in fade-in zoom-in duration-200 border border-gray-200 dark:border-gray-700">
        <div className="text-center mb-6">
          <div className="inline-flex items-center justify-center w-16 h-16 bg-blue-100 dark:bg-blue-900/30 rounded-full mb-4">
            <Play className="w-8 h-8 text-blue-600 dark:text-blue-400" />
          </div>
          <h2 
            id="resume-prompt-title"
            className="text-2xl font-bold text-gray-900 dark:text-white mb-2"
          >
            Resume Watching?
          </h2>
          <p className="text-gray-600 dark:text-gray-300">
            You've previously watched this lesson up to{' '}
            <span className="font-semibold text-blue-600 dark:text-blue-400">
              {formatSeconds(resumePosition)}
            </span>
          </p>
        </div>

        <div className="space-y-3">
          <button
            onClick={onResume}
            className="w-full flex items-center justify-center gap-3 px-6 py-4 bg-gradient-to-r from-blue-600 to-blue-700 text-white rounded-xl hover:from-blue-700 hover:to-blue-800 transition-all duration-200 shadow-lg hover:shadow-xl transform hover:scale-[1.02] active:scale-[0.98]"
            aria-label={`Resume from ${formatSeconds(resumePosition)}`}
          >
            <Play className="w-5 h-5" />
            <span className="font-semibold">Resume from {formatSeconds(resumePosition)}</span>
          </button>

          <button
            onClick={onStartFromBeginning}
            className="w-full flex items-center justify-center gap-3 px-6 py-4 bg-white dark:bg-gray-800 border-2 border-gray-200 dark:border-gray-700 text-gray-700 dark:text-gray-300 rounded-xl hover:bg-gray-50 dark:hover:bg-gray-700 hover:border-gray-300 dark:hover:border-gray-600 transition-all duration-200 transform hover:scale-[1.02] active:scale-[0.98]"
            aria-label="Start from beginning"
          >
            <RotateCcw className="w-5 h-5" />
            <span className="font-semibold">Start from Beginning</span>
          </button>
        </div>

        <div className="mt-6 pt-6 border-t border-gray-200 dark:border-gray-700">
          <div className="flex items-center justify-between text-sm text-gray-500 dark:text-gray-400">
            <span>Progress</span>
            <span className="font-medium">
              {formatSeconds(resumePosition)} / {formatSeconds(videoDuration)}
            </span>
          </div>
          <div className="mt-2 h-2 bg-gray-200 dark:bg-gray-700 rounded-full overflow-hidden">
            <div 
              className="h-full bg-gradient-to-r from-blue-500 to-blue-600 transition-all duration-300"
              style={{ width: `${Math.min((resumePosition / videoDuration) * 100, 100)}%` }}
              role="progressbar"
              aria-valuenow={Math.min((resumePosition / videoDuration) * 100, 100)}
              aria-valuemin={0}
              aria-valuemax={100}
            />
          </div>
        </div>
      </div>
    </div>
  );
}
