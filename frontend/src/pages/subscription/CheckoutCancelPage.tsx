import { useNavigate } from 'react-router-dom';
import { XCircle, ArrowLeft, RefreshCw } from 'lucide-react';

export function CheckoutCancelPage() {
  const navigate = useNavigate();

  return (
    <div className="flex min-h-screen items-center justify-center bg-gradient-to-b from-gray-50 to-white p-4">
      <div className="w-full max-w-md text-center">
        {/* Cancel Icon */}
        <div className="mb-6 flex justify-center">
          <div className="rounded-full bg-gray-100 p-6">
            <XCircle className="h-16 w-16 text-gray-600" />
          </div>
        </div>

        {/* Cancel Message */}
        <h1 className="mb-4 text-3xl font-extrabold text-gray-900">
          Checkout Cancelled
        </h1>
        <p className="mb-8 text-lg text-gray-600">
          No charges were made. You can return to pricing and try again anytime.
        </p>

        {/* Info Box */}
        <div className="mb-8 rounded-xl bg-blue-50 border border-blue-200 p-6 text-left">
          <h2 className="mb-2 text-lg font-semibold text-blue-900">
            Why Choose Premium?
          </h2>
          <ul className="space-y-2 text-sm text-blue-800">
            <li>• Unlimited access to all courses</li>
            <li>• Priority customer support</li>
            <li>• Advanced analytics and insights</li>
            <li>• Certificate of completion</li>
            <li>• 30-day money-back guarantee</li>
          </ul>
        </div>

        {/* Action Buttons */}
        <div className="flex flex-col gap-3 sm:flex-row sm:justify-center">
          <button
            onClick={() => navigate('/pricing')}
            className="inline-flex items-center justify-center gap-2 rounded-lg bg-blue-600 px-6 py-3 font-semibold text-white transition-colors hover:bg-blue-700"
          >
            <RefreshCw className="h-5 w-5" />
            Try Again
          </button>
          <button
            onClick={() => navigate('/courses')}
            className="inline-flex items-center justify-center gap-2 rounded-lg border-2 border-gray-300 bg-white px-6 py-3 font-semibold text-gray-900 transition-colors hover:bg-gray-50"
          >
            <ArrowLeft className="h-5 w-5" />
            Back to Courses
          </button>
        </div>

        {/* Support Link */}
        <p className="mt-8 text-sm text-gray-600">
          Need help?{' '}
          <a
            href="/support"
            className="font-medium text-blue-600 hover:text-blue-700"
          >
            Contact our support team
          </a>
        </p>
      </div>
    </div>
  );
}
