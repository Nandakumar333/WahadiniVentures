import * as React from 'react';
import { Check, X } from 'lucide-react';
import { cn } from '@/lib/utils';
import { Progress } from '@/components/ui/progress';

export interface PasswordStrengthIndicatorProps {
  password: string;
  showRequirements?: boolean;
  minLength?: number;
  className?: string;
}

interface PasswordRequirement {
  label: string;
  test: (password: string) => boolean;
}

interface PasswordStrength {
  score: number; // 0-4
  label: string;
  color: string;
  percentage: number;
}

export const PasswordStrengthIndicator: React.FC<PasswordStrengthIndicatorProps> = ({
  password,
  showRequirements = true,
  minLength = 8,
  className,
}) => {
  const requirements: PasswordRequirement[] = React.useMemo(
    () => [
      {
        label: `At least ${minLength} characters`,
        test: (pwd) => pwd.length >= minLength,
      },
      {
        label: 'Contains uppercase letter',
        test: (pwd) => /[A-Z]/.test(pwd),
      },
      {
        label: 'Contains lowercase letter',
        test: (pwd) => /[a-z]/.test(pwd),
      },
      {
        label: 'Contains number',
        test: (pwd) => /\d/.test(pwd),
      },
      {
        label: 'Contains special character',
        test: (pwd) => /[!@#$%^&*()_+\-=\[\]{};':"\\|,.<>\/?]/.test(pwd),
      },
    ],
    [minLength]
  );

  const calculateStrength = React.useCallback(
    (pwd: string): PasswordStrength => {
      if (!pwd) {
        return {
          score: 0,
          label: 'No password',
          color: 'bg-gray-300',
          percentage: 0,
        };
      }

      const metRequirements = requirements.filter((req) => req.test(pwd)).length;
      const score = Math.min(4, Math.floor((metRequirements / requirements.length) * 5));

      const strengthLevels: Record<number, { label: string; color: string }> = {
        0: { label: 'Very Weak', color: 'bg-red-500' },
        1: { label: 'Weak', color: 'bg-orange-500' },
        2: { label: 'Fair', color: 'bg-yellow-500' },
        3: { label: 'Good', color: 'bg-blue-500' },
        4: { label: 'Strong', color: 'bg-green-500' },
      };

      const level = strengthLevels[score];
      const percentage = (score / 4) * 100;

      return {
        score,
        label: level.label,
        color: level.color,
        percentage,
      };
    },
    [requirements]
  );

  const [strength, setStrength] = React.useState<PasswordStrength>(() =>
    calculateStrength(password)
  );

  // Debounced strength calculation
  React.useEffect(() => {
    const timer = setTimeout(() => {
      setStrength(calculateStrength(password));
    }, 150);

    return () => clearTimeout(timer);
  }, [password, calculateStrength]);

  return (
    <div className={cn('space-y-3', className)}>
      {/* Strength Bar */}
      <div className="space-y-2" role="status" aria-live="polite">
        <div className="flex items-center justify-between text-sm">
          <span className="text-muted-foreground">Password strength:</span>
          <span
            className={cn(
              'font-medium transition-colors',
              strength.score === 0 && 'text-gray-500',
              strength.score === 1 && 'text-red-500',
              strength.score === 2 && 'text-orange-500',
              strength.score === 3 && 'text-yellow-500',
              strength.score === 4 && 'text-blue-500',
              strength.score >= 4 && 'text-green-500'
            )}
          >
            {strength.label}
          </span>
        </div>
        
        <Progress 
          value={strength.percentage} 
          className="h-2"
          indicatorClassName={cn(
            'transition-all duration-300',
            strength.color
          )}
        />
      </div>

      {/* Requirements Checklist */}
      {showRequirements && (
        <div className="space-y-2">
          <p className="text-sm font-medium text-muted-foreground">
            Password must contain:
          </p>
          <ul className="space-y-1.5" role="list">
            {requirements.map((requirement, index) => {
              const isMet = requirement.test(password);
              return (
                <li
                  key={index}
                  className={cn(
                    'flex items-center gap-2 text-sm transition-colors',
                    isMet ? 'text-green-600 dark:text-green-500' : 'text-muted-foreground'
                  )}
                >
                  <span
                    className={cn(
                      'flex h-4 w-4 items-center justify-center rounded-full transition-colors',
                      isMet
                        ? 'bg-green-100 dark:bg-green-900/30'
                        : 'bg-muted'
                    )}
                  >
                    {isMet ? (
                      <Check className="h-3 w-3 text-green-600 dark:text-green-500" />
                    ) : (
                      <X className="h-3 w-3 text-muted-foreground" />
                    )}
                  </span>
                  <span>{requirement.label}</span>
                </li>
              );
            })}
          </ul>
        </div>
      )}
    </div>
  );
};
