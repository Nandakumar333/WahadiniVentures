import { useState } from 'react';
import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';
import { z } from 'zod';
import { Eye, EyeOff, Loader2 } from 'lucide-react';
import { Button } from '@/components/ui/button';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Alert, AlertDescription } from '@/components/ui/alert';
import { AuthService } from '@/services/authService';

// Validation schema
const resetPasswordSchema = z
  .object({
    password: z
      .string()
      .min(8, 'Password must be at least 8 characters')
      .regex(/[A-Z]/, 'Password must contain at least one uppercase letter')
      .regex(/[a-z]/, 'Password must contain at least one lowercase letter')
      .regex(/[0-9]/, 'Password must contain at least one number')
      .regex(/[^A-Za-z0-9]/, 'Password must contain at least one special character'),
    confirmPassword: z.string().min(1, 'Please confirm your password'),
  })
  .refine((data) => data.password === data.confirmPassword, {
    message: 'Passwords do not match',
    path: ['confirmPassword'],
  });

type ResetPasswordFormData = z.infer<typeof resetPasswordSchema>;

interface ResetPasswordFormProps {
  token: string;
  email: string;
  onSuccess?: () => void;
  onError?: (error: string) => void;
  className?: string;
}

export function ResetPasswordForm({
  token,
  email,
  onSuccess,
  onError,
  className,
}: ResetPasswordFormProps) {
  const [showPassword, setShowPassword] = useState(false);
  const [showConfirmPassword, setShowConfirmPassword] = useState(false);
  const [isLoading, setIsLoading] = useState(false);
  const [error, setError] = useState<string | null>(null);

  const {
    register,
    handleSubmit,
    formState: { errors },
    watch,
  } = useForm<ResetPasswordFormData>({
    resolver: zodResolver(resetPasswordSchema),
    defaultValues: {
      password: '',
      confirmPassword: '',
    },
  });

  const password = watch('password');

  // Password strength calculation
  const getPasswordStrength = (password: string) => {
    if (!password) return null;
    
    let strength = 0;
    const checks = [
      password.length >= 8,
      /[A-Z]/.test(password),
      /[a-z]/.test(password),
      /[0-9]/.test(password),
      /[^A-Za-z0-9]/.test(password),
    ];

    strength = checks.filter(Boolean).length;

    if (strength <= 2) return { level: 'weak', color: 'bg-red-500', text: 'Weak' };
    if (strength <= 3) return { level: 'medium', color: 'bg-yellow-500', text: 'Medium' };
    if (strength <= 4) return { level: 'strong', color: 'bg-blue-500', text: 'Strong' };
    return { level: 'very-strong', color: 'bg-green-500', text: 'Very Strong' };
  };

  const passwordStrength = getPasswordStrength(password);

  const onSubmit = async (data: ResetPasswordFormData) => {
    setIsLoading(true);
    setError(null);

    try {
      await AuthService.resetPassword({
        email,
        token,
        newPassword: data.password,
        confirmNewPassword: data.confirmPassword,
      });

      onSuccess?.();
    } catch (err) {
      const errorMessage =
        err instanceof Error
          ? err.message
          : 'Failed to reset password. The link may be expired or invalid.';
      setError(errorMessage);
      onError?.(errorMessage);
    } finally {
      setIsLoading(false);
    }
  };

  return (
    <div className={className}>
      <form onSubmit={handleSubmit(onSubmit)} className="space-y-6" noValidate>
        {/* Password Field */}
        <div className="space-y-2">
          <Label htmlFor="password">New Password</Label>
          <div className="relative">
            <Input
              id="password"
              type={showPassword ? 'text' : 'password'}
              placeholder="Enter your new password"
              {...register('password')}
              className={`pr-10 ${errors.password ? 'border-red-500' : ''}`}
              disabled={isLoading}
            />
            <button
              type="button"
              onClick={() => setShowPassword(!showPassword)}
              className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
              disabled={isLoading}
            >
              {showPassword ? (
                <EyeOff className="h-4 w-4" />
              ) : (
                <Eye className="h-4 w-4" />
              )}
            </button>
          </div>
          {errors.password && (
            <p className="text-sm text-red-600">{errors.password.message}</p>
          )}

          {/* Password Strength Indicator */}
          {password && passwordStrength && (
            <div className="mt-2">
              <div className="flex items-center justify-between text-sm">
                <span className="text-gray-600">Password strength:</span>
                <span
                  className={`font-medium ${
                    passwordStrength.level === 'weak'
                      ? 'text-red-600'
                      : passwordStrength.level === 'medium'
                      ? 'text-yellow-600'
                      : passwordStrength.level === 'strong'
                      ? 'text-blue-600'
                      : 'text-green-600'
                  }`}
                >
                  {passwordStrength.text}
                </span>
              </div>
              <div className="mt-1 flex space-x-1">
                {[1, 2, 3, 4, 5].map((level) => (
                  <div
                    key={level}
                    className={`h-2 w-full rounded-full ${
                      level <=
                      (passwordStrength.level === 'weak'
                        ? 1
                        : passwordStrength.level === 'medium'
                        ? 2
                        : passwordStrength.level === 'strong'
                        ? 3
                        : 4)
                        ? passwordStrength.color
                        : 'bg-gray-200'
                    }`}
                  />
                ))}
              </div>
            </div>
          )}
        </div>

        {/* Confirm Password Field */}
        <div className="space-y-2">
          <Label htmlFor="confirmPassword">Confirm New Password</Label>
          <div className="relative">
            <Input
              id="confirmPassword"
              type={showConfirmPassword ? 'text' : 'password'}
              placeholder="Confirm your new password"
              {...register('confirmPassword')}
              className={`pr-10 ${errors.confirmPassword ? 'border-red-500' : ''}`}
              disabled={isLoading}
            />
            <button
              type="button"
              onClick={() => setShowConfirmPassword(!showConfirmPassword)}
              className="absolute inset-y-0 right-0 flex items-center pr-3 text-gray-400 hover:text-gray-600"
              disabled={isLoading}
            >
              {showConfirmPassword ? (
                <EyeOff className="h-4 w-4" />
              ) : (
                <Eye className="h-4 w-4" />
              )}
            </button>
          </div>
          {errors.confirmPassword && (
            <p className="text-sm text-red-600">{errors.confirmPassword.message}</p>
          )}
        </div>

        {/* Error Message */}
        {error && (
          <Alert variant="destructive">
            <AlertDescription>{error}</AlertDescription>
          </Alert>
        )}

        {/* Submit Button */}
        <Button type="submit" className="w-full" disabled={isLoading}>
          {isLoading && <Loader2 className="mr-2 h-4 w-4 animate-spin" />}
          {isLoading ? 'Resetting Password...' : 'Reset Password'}
        </Button>

        {/* Password Requirements */}
        <div className="bg-gray-50 p-4 rounded-md">
          <p className="text-sm font-medium text-gray-700 mb-2">Password must contain:</p>
          <ul className="text-xs text-gray-600 space-y-1">
            <li className="flex items-center">
              <span className={password && password.length >= 8 ? 'text-green-600' : ''}>
                • At least 8 characters
              </span>
            </li>
            <li className="flex items-center">
              <span className={password && /[A-Z]/.test(password) ? 'text-green-600' : ''}>
                • One uppercase letter
              </span>
            </li>
            <li className="flex items-center">
              <span className={password && /[a-z]/.test(password) ? 'text-green-600' : ''}>
                • One lowercase letter
              </span>
            </li>
            <li className="flex items-center">
              <span className={password && /[0-9]/.test(password) ? 'text-green-600' : ''}>
                • One number
              </span>
            </li>
            <li className="flex items-center">
              <span className={password && /[^A-Za-z0-9]/.test(password) ? 'text-green-600' : ''}>
                • One special character
              </span>
            </li>
          </ul>
        </div>
      </form>
    </div>
  );
}

export default ResetPasswordForm;
