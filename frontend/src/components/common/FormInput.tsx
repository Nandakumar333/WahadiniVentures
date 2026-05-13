import * as React from 'react';
import { Eye, EyeOff, AlertCircle } from 'lucide-react';
import { Input } from '@/components/ui/input';
import { Label } from '@/components/ui/label';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export interface FormInputProps extends Omit<React.InputHTMLAttributes<HTMLInputElement>, 'prefix'> {
  label?: string;
  error?: string;
  helperText?: string;
  required?: boolean;
  prefix?: React.ReactNode;
  suffix?: React.ReactNode;
  showCharCount?: boolean;
  containerClassName?: string;
}

export const FormInput = React.forwardRef<HTMLInputElement, FormInputProps>(
  (
    {
      label,
      error,
      helperText,
      required,
      prefix,
      suffix,
      showCharCount,
      containerClassName,
      className,
      type = 'text',
      maxLength,
      id,
      ...props
    },
    ref
  ) => {
    const [showPassword, setShowPassword] = React.useState(false);
    const [charCount, setCharCount] = React.useState(0);
    const inputId = id || React.useId();
    const errorId = `${inputId}-error`;
    const helperId = `${inputId}-helper`;

    const isPasswordType = type === 'password';
    const actualType = isPasswordType && showPassword ? 'text' : type;

    const handleInputChange = (e: React.ChangeEvent<HTMLInputElement>) => {
      if (showCharCount) {
        setCharCount(e.target.value.length);
      }
      props.onChange?.(e);
    };

    return (
      <div className={cn('space-y-2', containerClassName)}>
        {/* Label */}
        {label && (
          <Label htmlFor={inputId} className="text-sm font-medium">
            {label}
            {required && <span className="ml-1 text-destructive" aria-label="required">*</span>}
          </Label>
        )}

        {/* Input Container */}
        <div className="relative">
          {/* Prefix */}
          {prefix && (
            <div className="absolute left-3 top-1/2 -translate-y-1/2 text-muted-foreground">
              {prefix}
            </div>
          )}

          {/* Input */}
          <Input
            ref={ref}
            id={inputId}
            type={actualType}
            maxLength={maxLength}
            aria-invalid={!!error}
            aria-describedby={cn(
              error && errorId,
              helperText && helperId
            )}
            aria-required={required}
            className={cn(
              prefix && 'pl-10',
              (suffix || isPasswordType) && 'pr-10',
              error && 'border-destructive focus-visible:ring-destructive',
              className
            )}
            onChange={handleInputChange}
            {...props}
          />

          {/* Suffix or Password Toggle */}
          <div className="absolute right-3 top-1/2 -translate-y-1/2">
            {isPasswordType ? (
              <Button
                type="button"
                variant="ghost"
                size="icon"
                className="h-7 w-7 hover:bg-transparent"
                onClick={() => setShowPassword(!showPassword)}
                aria-label={showPassword ? 'Hide password' : 'Show password'}
                tabIndex={-1}
              >
                {showPassword ? (
                  <EyeOff className="h-4 w-4 text-muted-foreground" />
                ) : (
                  <Eye className="h-4 w-4 text-muted-foreground" />
                )}
              </Button>
            ) : (
              suffix && <div className="text-muted-foreground">{suffix}</div>
            )}
          </div>
        </div>

        {/* Character Count */}
        {showCharCount && maxLength && (
          <div className="flex justify-end">
            <span
              className={cn(
                'text-xs',
                charCount > maxLength * 0.9
                  ? 'text-destructive'
                  : 'text-muted-foreground'
              )}
            >
              {charCount}/{maxLength}
            </span>
          </div>
        )}

        {/* Error Message */}
        {error && (
          <div
            id={errorId}
            className="flex items-center gap-1 text-sm text-destructive"
            role="alert"
          >
            <AlertCircle className="h-4 w-4" />
            <span>{error}</span>
          </div>
        )}

        {/* Helper Text */}
        {helperText && !error && (
          <p
            id={helperId}
            className="text-sm text-muted-foreground"
          >
            {helperText}
          </p>
        )}
      </div>
    );
  }
);

FormInput.displayName = 'FormInput';
