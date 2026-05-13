import { useState } from 'react';
import { useSearchParams, useNavigate, Link } from 'react-router-dom';
import { CheckCircle, XCircle } from 'lucide-react';
import { ResetPasswordForm } from '@/components/auth/ResetPasswordForm';
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from '@/components/ui/card';
import { Button } from '@/components/ui/button';

export function ResetPasswordPage() {
  const [searchParams] = useSearchParams();
  const navigate = useNavigate();
  const [success, setSuccess] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const token = searchParams.get('token');
  const email = searchParams.get('email');

  const handleSuccess = () => {
    setSuccess(true);
    setError(null);
    
    // Redirect to login after 3 seconds
    setTimeout(() => {
      navigate('/login', {
        state: { message: 'Password reset successful! Please log in with your new password.' },
      });
    }, 3000);
  };

  const handleError = (errorMessage: string) => {
    setError(errorMessage);
  };

  // Invalid link - missing token or email
  if (!token || !email) {
    return (
      <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
        <div className="max-w-md w-full">
          <div className="text-center mb-8">
            <h1 className="text-3xl font-bold text-gray-900">WahadiniCryptoQuest</h1>
          </div>

          <Card>
            <CardHeader>
              <div className="flex justify-center mb-4">
                <div className="rounded-full bg-red-100 p-3">
                  <XCircle className="h-6 w-6 text-red-600" />
                </div>
              </div>
              <CardTitle className="text-center">Invalid Reset Link</CardTitle>
              <CardDescription className="text-center">
                This password reset link is invalid or incomplete.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-gray-600 text-center">
                Please request a new password reset link or contact support if you continue to
                experience issues.
              </p>

              <div className="flex flex-col space-y-3">
                <Button asChild>
                  <Link to="/forgot-password">Request New Reset Link</Link>
                </Button>
                <Button variant="outline" asChild>
                  <Link to="/login">Back to Login</Link>
                </Button>
              </div>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  // Success state
  if (success) {
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
              <CardTitle className="text-center">Password Reset Successful!</CardTitle>
              <CardDescription className="text-center">
                Your password has been successfully reset.
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <p className="text-sm text-gray-600 text-center">
                You will be redirected to the login page shortly, or you can click the button
                below.
              </p>

              <Button asChild className="w-full">
                <Link to="/login">Continue to Login</Link>
              </Button>
            </CardContent>
          </Card>
        </div>
      </div>
    );
  }

  // Normal reset form
  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full">
        <div className="text-center mb-8">
          <h1 className="text-3xl font-bold text-gray-900">WahadiniCryptoQuest</h1>
          <p className="text-gray-600 mt-2">Create your new password</p>
        </div>

        <Card>
          <CardHeader>
            <CardTitle>Reset Your Password</CardTitle>
            <CardDescription>
              Enter a new password for your account: <strong>{email}</strong>
            </CardDescription>
          </CardHeader>
          <CardContent>
            <ResetPasswordForm
              token={token}
              email={email}
              onSuccess={handleSuccess}
              onError={handleError}
            />
          </CardContent>
        </Card>

        {error && (
          <div className="mt-4 bg-red-50 border border-red-200 rounded-lg p-4">
            <p className="text-sm text-red-800">{error}</p>
            <div className="mt-3">
              <Link
                to="/forgot-password"
                className="text-sm text-red-600 hover:text-red-500 underline font-medium"
              >
                Request a new reset link
              </Link>
            </div>
          </div>
        )}

        <div className="mt-6 text-center">
          <Link to="/login" className="text-sm text-blue-600 hover:text-blue-500 underline">
            Back to login
          </Link>
        </div>
      </div>
    </div>
  );
}

export default ResetPasswordPage;
