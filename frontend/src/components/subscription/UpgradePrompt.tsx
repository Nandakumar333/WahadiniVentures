import React from 'react';

interface UpgradePromptProps {
  isOpen: boolean;
  onClose: () => void;
  courseTitle?: string;
}

/**
 * Premium upgrade modal component
 * Shown to free users when they try to enroll in premium courses
 */
export const UpgradePrompt: React.FC<UpgradePromptProps> = ({
  isOpen,
  onClose,
  courseTitle,
}) => {
  if (!isOpen) return null;

  return (
    <div className="fixed inset-0 z-50 overflow-y-auto" aria-labelledby="modal-title" role="dialog" aria-modal="true">
      {/* Backdrop */}
      <div 
        className="fixed inset-0 bg-black bg-opacity-50 transition-opacity"
        onClick={onClose}
      ></div>

      {/* Modal */}
      <div className="flex min-h-screen items-center justify-center p-4">
        <div className="relative w-full max-w-lg transform overflow-hidden rounded-2xl bg-white dark:bg-gray-800 shadow-2xl transition-all border border-gray-200 dark:border-gray-700">
          {/* Close button */}
          <button
            type="button"
            onClick={onClose}
            className="absolute top-4 right-4 text-gray-400 hover:text-gray-600 dark:hover:text-gray-300 transition-colors"
          >
            <svg
              className="h-6 w-6"
              fill="none"
              viewBox="0 0 24 24"
              stroke="currentColor"
            >
              <path
                strokeLinecap="round"
                strokeLinejoin="round"
                strokeWidth={2}
                d="M6 18L18 6M6 6l12 12"
              />
            </svg>
          </button>

          {/* Content */}
          <div className="p-8">
            {/* Icon */}
            <div className="mx-auto flex h-16 w-16 items-center justify-center rounded-full bg-gradient-to-r from-yellow-400 to-yellow-600">
              <svg
                className="h-8 w-8 text-white"
                fill="currentColor"
                viewBox="0 0 20 20"
                xmlns="http://www.w3.org/2000/svg"
              >
                <path
                  fillRule="evenodd"
                  d="M5 9V7a5 5 0 0110 0v2a2 2 0 012 2v5a2 2 0 01-2 2H5a2 2 0 01-2-2v-5a2 2 0 012-2zm8-2v2H7V7a3 3 0 016 0z"
                  clipRule="evenodd"
                />
              </svg>
            </div>

            {/* Title */}
            <h3 className="mt-6 text-center text-2xl font-bold text-gray-900 dark:text-white" id="modal-title">
              Premium Course Access Required
            </h3>

            {/* Description */}
            <p className="mt-4 text-center text-gray-600 dark:text-gray-400">
              {courseTitle ? (
                <>
                  <span className="font-semibold">{courseTitle}</span> is a premium course.
                </>
              ) : (
                'This is a premium course.'
              )}{' '}
              Upgrade to Premium to access this course and unlock exclusive content!
            </p>

            {/* Benefits */}
            <div className="mt-6 space-y-3">
              <div className="flex items-start">
                <svg
                  className="mt-1 h-5 w-5 flex-shrink-0 text-green-500"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                  />
                </svg>
                <span className="ml-3 text-sm text-gray-700 dark:text-gray-300">
                  Access to <strong>all premium courses</strong>
                </span>
              </div>
              <div className="flex items-start">
                <svg
                  className="mt-1 h-5 w-5 flex-shrink-0 text-green-500"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                  />
                </svg>
                <span className="ml-3 text-sm text-gray-700 dark:text-gray-300">
                  <strong>Exclusive</strong> advanced lessons and projects
                </span>
              </div>
              <div className="flex items-start">
                <svg
                  className="mt-1 h-5 w-5 flex-shrink-0 text-green-500"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                  />
                </svg>
                <span className="ml-3 text-sm text-gray-700 dark:text-gray-300">
                  <strong>Priority support</strong> from instructors
                </span>
              </div>
              <div className="flex items-start">
                <svg
                  className="mt-1 h-5 w-5 flex-shrink-0 text-green-500"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                  />
                </svg>
                <span className="ml-3 text-sm text-gray-700 dark:text-gray-300">
                  <strong>Certificate of completion</strong> for all courses
                </span>
              </div>
              <div className="flex items-start">
                <svg
                  className="mt-1 h-5 w-5 flex-shrink-0 text-green-500"
                  fill="currentColor"
                  viewBox="0 0 20 20"
                  xmlns="http://www.w3.org/2000/svg"
                >
                  <path
                    fillRule="evenodd"
                    d="M10 18a8 8 0 100-16 8 8 0 000 16zm3.707-9.293a1 1 0 00-1.414-1.414L9 10.586 7.707 9.293a1 1 0 00-1.414 1.414l2 2a1 1 0 001.414 0l4-4z"
                    clipRule="evenodd"
                  />
                </svg>
                <span className="ml-3 text-sm text-gray-700 dark:text-gray-300">
                  <strong>Ad-free</strong> learning experience
                </span>
              </div>
            </div>

            {/* Pricing hint */}
            <div className="mt-6 rounded-lg bg-gradient-to-r from-yellow-50 to-yellow-100 dark:from-yellow-900/20 dark:to-yellow-800/20 p-4 text-center">
              <p className="text-sm text-gray-700 dark:text-gray-300">
                Starting at <span className="text-xl font-bold text-gray-900 dark:text-white">$9.99/month</span>
              </p>
            </div>

            {/* Actions */}
            <div className="mt-8 flex flex-col gap-3 sm:flex-row-reverse">
              <a
                href="/pricing"
                className="inline-flex w-full justify-center rounded-lg bg-gradient-to-r from-yellow-400 to-yellow-600 px-6 py-3 text-base font-semibold text-gray-900 shadow-lg hover:from-yellow-500 hover:to-yellow-700 transition-all duration-200 sm:w-auto"
              >
                View Pricing Plans
              </a>
              <button
                type="button"
                onClick={onClose}
                className="inline-flex w-full justify-center rounded-lg border border-gray-300 dark:border-gray-600 bg-white dark:bg-gray-700 px-6 py-3 text-base font-semibold text-gray-700 dark:text-gray-200 hover:bg-gray-50 dark:hover:bg-gray-600 transition-colors duration-200 sm:w-auto"
              >
                Maybe Later
              </button>
            </div>
          </div>
        </div>
      </div>
    </div>
  );
};
