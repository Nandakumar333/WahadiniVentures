import { useState } from 'react';
import { Link } from 'react-router-dom';
import { CheckCircle } from 'lucide-react';
import { ForgotPasswordForm } from '@/components/auth/ForgotPasswordForm';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';

export function ForgotPasswordPage() {
  const [submittedEmail, setSubmittedEmail] = useState<string | null>(null);

  const handleSuccess = (email: string) => {
    setSubmittedEmail(email);
  };

  if (submittedEmail) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-md w-full">
          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-gray-900">WahadiniCryptoQuest</h1>
          </div>

          <Card>
            <CardHeader>
              <div className="flex justify-center mb-4">
                <div className="rounded-full bg-green-100 p-3">
                  <CheckCircle className="h-6 w-6 text-green-600" />
                </div>
              </div>
              <CardTitle className="text-center">Check Your Email</CardTitle>
              <CardDescription className="text-center">
                We've sent password reset instructions to
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              <div className="text-center">
                <p className="font-medium text-gray-900">{submittedEmail}</p>
              </div>

              <div className="bg-blue-50 border border-blue-200 rounded-lg p-4">
                <p className="text-sm text-blue-900">
                  <strong>Next steps:</strong>
                </p>
                <ol className="text-sm text-blue-800 mt-2 space-y-1 list-decimal list-inside">
                  <li>Check your email inbox (and spam folder)</li>
                  <li>Click the password reset link in the email</li>
                  <li>Create your new password</li>
                </ol>
              </div>

              <p className="text-xs text-gray-500 text-center">
                Didn't receive the email?{' '}
                <button
                  onClick={() => setSubmittedEmail(null)}
                  className="text-blue-600 hover:text-blue-500 underline"
                >
                  Try again
                </button>
              </p>

              <div className="text-center pt-4 border-t">
                <Link
                  to="/login"
                  className="text-sm text-blue-600 hover:text-blue-500 underline"
                >
                  Back to login
                </Link>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">WahadiniCryptoQuest</h1>
          <p className="text-gray-600 mt-2">Reset your password</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Forgot Password?</CardTitle>
            <CardDescription>
              Enter your email address and we'll send you instructions to reset your password.
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ForgotPasswordForm onSuccess={handleSuccess} />
          </CardContent>
        </Card>

        <div className="mt-6 text-center">
          <Link
            to="/login"
            className="text-sm text-blue-600 hover:text-blue-500 underline"
          >
            Back to login
          </Link>
        </div>
      </div>
    </div>
  );
}

export default ForgotPasswordPage;
