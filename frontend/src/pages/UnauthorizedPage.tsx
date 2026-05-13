import { useNavigate } from 'react-router-dom';
import { Button } from '@/components/ui/button';
import { Card, CardContent } from '@/components/ui/card';
import { Shield, Home, ArrowLeft } from 'lucide-react';

/**
 * Unauthorized page displayed when user tries to access admin-only content
 */
export const UnauthorizedPage = () => {
  const navigate = useNavigate();

  return (
    <div className="min-h-screen flex items-center justify-center bg-gradient-to-br from-gray-50 to-gray-100 p-4">
      <Card className="max-w-md w-full">
        <CardContent className="pt-12 pb-8 px-8 text-center">
          {/* Icon */}
          <div className="mx-auto mb-6 h-20 w-20 rounded-full bg-red-100 flex items-center justify-center">
            <Shield className="h-10 w-10 text-red-600" />
          </div>

          {/* Title */}
          <h1 className="text-3xl font-bold text-gray-900 mb-3">
            Access Denied
          </h1>

          {/* Message */}
          <p className="text-gray-600 mb-2">
            You don't have permission to access this page.
          </p>
          <p className="text-gray-600 mb-8">
            This area is restricted to administrators only.
          </p>

          {/* Actions */}
          <div className="flex flex-col sm:flex-row gap-3 justify-center">
            <Button
              variant="outline"
              onClick={() => navigate(-1)}
              className="flex items-center gap-2"
            >
              <ArrowLeft className="h-4 w-4" />
              Go Back
            </Button>
            <Button
              onClick={() => navigate('/dashboard')}
              className="flex items-center gap-2"
            >
              <Home className="h-4 w-4" />
              Go to Dashboard
            </Button>
          </div>

          {/* Help Text */}
          <p className="mt-8 text-sm text-gray-500">
            If you believe you should have access, please contact support.
          </p>
        </CardContent>
      </Card>
    </div>
  );
};

export default UnauthorizedPage;
