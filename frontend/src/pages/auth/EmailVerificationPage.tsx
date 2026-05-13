import { useEffect, useState } from 'react';
import { useSearchParams, Link } from 'react-router-dom';
import { Loader2, CheckCircle, XCircle } from 'lucide-react';
import { AuthService } from '@/services/authService';
import { Button } from '@/components/ui/button';

type VerificationStatus = 'loading' | 'success' | 'error' | 'invalid';

export function EmailVerificationPage() {
  const [searchParams] = useSearchParams();
  const [status, setStatus] = useState<VerificationStatus>('loading');
  const [message, setMessage] = useState<string>('');
  
  const userId = searchParams.get('userId');
  const token = searchParams.get('token');

  useEffect(() => {
    const verifyEmail = async () => {
      if (!userId || !token) {
        setStatus('invalid');
        setMessage('Invalid verification link. Please check your email for the correct link.');
        return;
      }

      try {
        setStatus('loading');
        await AuthService.confirmEmail({ userId, token });
        setStatus('success');
        setMessage('Your email has been successfully verified! You can now log in to your account.');
      } catch (error) {
        setStatus('error');
        const errorMessage = error instanceof Error ? error.message : 'Email verification failed. The link may be expired or invalid.';
        setMessage(errorMessage);
      }
    };

    verifyEmail();
  }, [userId, token]);

  const renderContent = () => {
    switch (status) {
      case 'loading':
        return (
          <div className="text-center">
            <Loader2 className="mx-auto h-12 w-12 text-indigo-600 animate-spin" />
            <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
              Verifying your email...
            </h2>
            <p className="mt-2 text-sm text-gray-600">
              Please wait while we verify your email address.
            </p>
          </div>
        );

      case 'success':
        return (
          <div className="text-center">
            <CheckCircle className="mx-auto h-12 w-12 text-green-600" />
            <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
              Email Verified!
            </h2>
            <p className="mt-2 text-sm text-gray-600">
              {message}
            </p>
            <div className="mt-6">
              <Link to="/login">
                <Button>
                  Continue to Login
                </Button>
              </Link>
            </div>
          </div>
        );

      case 'error':
        return (
          <div className="text-center">
            <XCircle className="mx-auto h-12 w-12 text-red-600" />
            <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
              Verification Failed
            </h2>
            <p className="mt-2 text-sm text-gray-600">
              {message}
            </p>
            <div className="mt-6 space-y-3">
              <div>
                <Button variant="outline" asChild>
                  <Link to="/register">
                    Try Registration Again
                  </Link>
                </Button>
              </div>
              <div>
                <Button variant="secondary" asChild>
                  <Link to="/login">
                    Back to Login
                  </Link>
                </Button>
              </div>
            </div>
          </div>
        );

      case 'invalid':
        return (
          <div className="text-center">
            <XCircle className="mx-auto h-12 w-12 text-red-600" />
            <h2 className="mt-6 text-3xl font-extrabold text-gray-900">
              Invalid Link
            </h2>
            <p className="mt-2 text-sm text-gray-600">
              {message}
            </p>
            <div className="mt-6 space-y-3">
              <div>
                <Button asChild>
                  <Link to="/register">
                    Register for New Account
                  </Link>
                </Button>
              </div>
              <div>
                <Button variant="outline" asChild>
                  <Link to="/login">
                    Back to Login
                  </Link>
                </Button>
              </div>
            </div>
          </div>
        );

      default:
        return null;
    }
  };

  return (
    <div className="min-h-screen flex items-center justify-center bg-gray-50 py-12 px-4 sm:px-6 lg:px-8">
      <div className="max-w-md w-full space-y-8">
        <div>
          <div className="mx-auto h-12 w-auto flex items-center justify-center">
            <h1 className="text-2xl font-bold text-indigo-600">WahadiniCryptoQuest</h1>
          </div>
        </div>

        <div className="bg-white py-8 px-4 shadow sm:rounded-lg sm:px-10">
          {renderContent()}
        </div>

        <div className="text-center">
          <p className="text-xs text-gray-500">
            Need help?{' '}
            <a href="#" className="text-indigo-600 hover:text-indigo-500">
              Contact support
            </a>
          </p>
        </div>
      </div>
    </div>
  );
}