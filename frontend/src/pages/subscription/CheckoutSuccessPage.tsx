import { useEffect } from 'react';
import { useNavigate, useSearchParams } from 'react-router-dom';
import { CheckCircle, ArrowRight } from 'lucide-react';
import { useQueryClient } from '@tanstack/react-query';

export function CheckoutSuccessPage() {
  const navigate = useNavigate();
  const [searchParams] = useSearchParams();
  const queryClient = useQueryClient();
  const sessionId = searchParams.get('session_id');

  useEffect(() => {
    // Invalidate subscription cache to fetch updated status
    queryClient.invalidateQueries({ queryKey: ['subscription'] });
  }, [queryClient]);

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-green-50 to-white p-4">
      <div className="w-full max-w-md text-center">
        {/* Success Icon */}
        <div className="mb-6 flex justify-center">
          <div className="rounded-full bg-green-100 p-6">
            <CheckCircle className="h-16 w-16 text-green-600" />
          </div>
        </div>

        {/* Success Message */}
        <h1 className="mb-4 text-3xl font-extrabold text-gray-900">
          Welcome to Premium! 🎉
        </h1>
        <p className="mb-8 text-lg text-gray-600">
          Your subscription has been activated successfully. You now have access to all premium features.
        </p>

        {/* Session Info */}
        {sessionId && (
          <div className="mb-8 rounded-lg bg-gray-50 border border-gray-200 p-4">
            <p className="text-sm text-gray-600">
              Order ID:{' '}
              <span className="font-mono text-gray-900">{sessionId.substring(0, 20)}...</span>
            </p>
          </div>
        )}

        {/* What's Next */}
        <div className="mb-8 rounded-xl bg-white border border-gray-200 p-6 shadow-sm text-left">
          <h2 className="mb-4 text-lg font-semibold text-gray-900">What's Next?</h2>
          <ul className="space-y-3">
            <li className="flex items-start gap-3">
              <CheckCircle className="h-5 w-5 flex-shrink-0 text-green-500 mt-0.5" />
              <span className="text-gray-700">
                Access all premium courses and content
              </span>
            </li>
            <li className="flex items-start gap-3">
              <CheckCircle className="h-5 w-5 flex-shrink-0 text-green-500 mt-0.5" />
              <span className="text-gray-700">
                Track your progress in the analytics dashboard
              </span>
            </li>
            <li className="flex items-start gap-3">
              <CheckCircle className="h-5 w-5 flex-shrink-0 text-green-500 mt-0.5" />
              <span className="text-gray-700">
                Get priority support from our team
              </span>
            </li>
          </ul>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
          <button
            onClick={() => navigate('/courses')}
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            Explore Courses
            <ArrowRight className="h-5 w-5" />
          </button>
          <button
            onClick={() => navigate('/dashboard')}
            className="inline-flex items-center justify-center gap-2 rounded-lg border-2 border-gray-300 bg-white px-6 py-3 font-semibold text-gray-900 transition-colors hover:bg-gray-50"
          >
            Go to Dashboard
          </button>
        </div>

        {/* Receipt Email Notice */}
        <p className="mt-8 text-sm text-gray-600">
          A receipt has been sent to your email address.
        </p>
      </div>
    </div>
  );
}
