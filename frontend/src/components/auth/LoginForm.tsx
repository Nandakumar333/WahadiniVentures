import React from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Eye, EyeOff, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Checkbox } from '@/components/ui/checkbox';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { useAuthStore } from '@/store/authStore';
import { useProcessStreak } from '@/hooks/reward';

// Validation schema for login form
const loginSchema = z.object({
  email: z
    .string()
    .min(1, 'Email is required')
    .email('Please enter a valid email address'),
  password: z
    .string()
    .min(1, 'Password is required'),
  rememberMe: z.boolean(),
});

type LoginFormData = z.infer<typeof loginSchema>;

interface LoginFormProps {
  onSuccess?: () => void;
  onRegisterClick?: () => void;
  onForgotPasswordClick?: () => void;
  className?: string;
}

export const LoginForm: React.FC<LoginFormProps> = ({
  onSuccess,
  onRegisterClick,
  onForgotPasswordClick,
  className,
}) => {
  const [showPassword, setShowPassword] = React.useState(false);
  const { login, isLoading, error, setError } = useAuthStore();
  const processStreak = useProcessStreak();

  const {
    register,
    handleSubmit,
    formState: { errors, isSubmitting },
  } = useForm<LoginFormData>({
    resolver: zodResolver(loginSchema),
    defaultValues: {
      email: '',
      password: '',
      rememberMe: false,
    },
  });

  const onSubmit = async (data: LoginFormData) => {
    try {
      // Clear any previous errors
      setError(null);

      const success = await login({
        email: data.email,
        password: data.password,
        rememberMe: data.rememberMe,
      });

      if (success) {
        // Process daily streak after successful login
        // This will track consecutive logins and award bonus points
        processStreak.mutate(undefined, {
          onError: (error) => {
            // Log error but don't block login flow
            console.error('Failed to process streak:', error);
          },
        });
        
        onSuccess?.();
      }
    } catch (error) {
      console.error('Login error:', error);
    }
  };

  const togglePasswordVisibility = () => {
    setShowPassword(!showPassword);
  };

  return (
    <div className={className}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6">
        {/* Email Field */}
        <div className="space-y-2">
          <Label htmlFor="email">Email Address</Label>
          <Input
            id="email"
            type="email"
            placeholder="Enter your email"
            {...register('email')}
            className={errors.email ? 'border-red-500' : ''}
            disabled={isLoading || isSubmitting}
            aria-invalid={errors.email ? 'true' : 'false'}
            aria-describedby={errors.email ? 'email-error' : undefined}
            autoComplete="email"
          />
          {errors.email && (
            <p id="email-error" className="text-sm text-red-600" role="alert" aria-live="polite">
              {errors.email.message}
            </p>
          )}
        </div>

        {/* Password Field */}
        <div className="space-y-2">
          <Label htmlFor="password">Password</Label>
          <div className="relative">
            <Input
              id="password"
              type={showPassword ? 'text' : 'password'}
              placeholder="Enter your password"
              {...register('password')}
              className={errors.password ? 'border-red-500 pr-10' : 'pr-10'}
              disabled={isLoading || isSubmitting}
              aria-invalid={errors.password ? 'true' : 'false'}
              aria-describedby={errors.password ? 'password-error' : undefined}
              autoComplete="current-password"
            />
            <button
              type="button"
              onClick={togglePasswordVisibility}
              className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
              disabled={isLoading || isSubmitting}
              aria-label={showPassword ? 'Hide password' : 'Show password'}
            >
              {showPassword ? (
                <EyeOff className="h-4 w-4" aria-hidden="true" />
              ) : (
                <Eye className="h-4 w-4" aria-hidden="true" />
              )}
            </button>
          </div>
          {errors.password && (
            <p id="password-error" className="text-sm text-red-600" role="alert" aria-live="polite">
              {errors.password.message}
            </p>
          )}
        </div>

        {/* Remember Me Checkbox */}
        <div className="flex items-center space-x-2">
          <Checkbox
            id="rememberMe"
            {...register('rememberMe')}
            disabled={isLoading || isSubmitting}
          />
          <Label
            htmlFor="rememberMe"
            className="text-sm font-medium leading-none peer-disabled:cursor-not-allowed peer-disabled:opacity-70"
          >
            Remember me for 30 days
          </Label>
        </div>

        {/* Error Display */}
        {error && (
          <Alert variant="destructive" role="alert" aria-live="assertive">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {/* Submit Button */}
        <Button
          type="submit"
          className="w-full"
          disabled={isLoading || isSubmitting}
          aria-busy={isLoading || isSubmitting}
        >
          {(isLoading || isSubmitting) && (
            <Loader2 className="mr-2 h-4 w-4 animate-spin" aria-hidden="true" />
          )}
          Sign In
        </Button>

        {/* Links */}
        <div className="space-y-4 text-center">
          {/* Forgot Password Link */}
          {onForgotPasswordClick && (
            <button
              type="button"
              onClick={onForgotPasswordClick}
              className="text-sm text-blue-600 hover:text-blue-500 underline"
              disabled={isLoading || isSubmitting}
            >
              Forgot your password?
            </button>
          )}

          {/* Register Link */}
          {onRegisterClick && (
            <div className="text-sm text-gray-600">
              Don't have an account?{' '}
              <button
                type="button"
                onClick={onRegisterClick}
                className="text-blue-600 hover:text-blue-500 underline font-medium"
                disabled={isLoading || isSubmitting}
              >
                Sign up
              </button>
            </div>
          )}
        </div>
      </form>
    </div>
  );
};

export default LoginForm;