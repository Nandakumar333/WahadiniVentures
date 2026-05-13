import { Loader2 } from 'lucide-react'
import { cn } from '@/lib/utils'

interface LoadingSpinnerProps {
  variant?: 'spinner' | 'dots' | 'pulse' | 'progress'
  size?: 'sm' | 'md' | 'lg' | 'xl'
  label?: string
  className?: string
  progress?: number // For progress variant (0-100)
  color?: string // Custom color class
}

export function LoadingSpinner({ 
  variant = 'spinner', 
  size = 'md', 
  label = 'Loading...', 
  className,
  progress,
  color = 'text-primary'
}: LoadingSpinnerProps) {
  const sizeClasses = {
    sm: 'h-4 w-4',
    md: 'h-8 w-8',
    lg: 'h-12 w-12',
    xl: 'h-16 w-16'
  }

  const dotSizes = {
    sm: 'h-1.5 w-1.5',
    md: 'h-2.5 w-2.5',
    lg: 'h-4 w-4',
    xl: 'h-5 w-5'
  }

  const renderSpinner = () => {
    switch (variant) {
      case 'spinner':
        return (
          <Loader2 
            className={cn('animate-spin', color, sizeClasses[size])} 
            aria-hidden="true"
          />
        )
      
      case 'dots':
        return (
          <div className="flex gap-2" aria-hidden="true">
            <div className={cn(dotSizes[size], color, 'rounded-full bg-current animate-bounce')} style={{ animationDelay: '0ms' }} />
            <div className={cn(dotSizes[size], color, 'rounded-full bg-current animate-bounce')} style={{ animationDelay: '150ms' }} />
            <div className={cn(dotSizes[size], color, 'rounded-full bg-current animate-bounce')} style={{ animationDelay: '300ms' }} />
          </div>
        )
      
      case 'pulse':
        return (
          <div 
            className={cn('rounded-full bg-current animate-pulse', color, sizeClasses[size])} 
            aria-hidden="true"
          />
        )
      
      case 'progress':
        return (
          <div className="w-full max-w-xs space-y-2" aria-hidden="true">
            <div className="h-2 w-full bg-secondary rounded-full overflow-hidden">
              <div 
                className={cn('h-full bg-current transition-all duration-300', color)}
                style={{ width: `${progress || 0}%` }}
              />
            </div>
            {progress !== undefined && (
              <p className="text-sm text-center text-muted-foreground">
                {Math.round(progress)}%
              </p>
            )}
          </div>
        )
      
      default:
        return null
    }
  }

  return (
    <div className={cn('flex flex-col items-center justify-center gap-2', className)} role="status" aria-live="polite">
      {renderSpinner()}
      <span className="sr-only">{label}</span>
      {label && variant !== 'progress' && (
        <p className="text-sm text-muted-foreground">
          {label}
        </p>
      )}
    </div>
  )
}

interface PageLoadingProps {
  message?: string
  variant?: 'spinner' | 'dots' | 'pulse' | 'progress'
  progress?: number
}

export function PageLoading({ message = 'Loading page...', variant = 'spinner', progress }: PageLoadingProps) {
  return (
    <div className="min-h-screen flex items-center justify-center">
      <LoadingSpinner size="lg" label={message} variant={variant} progress={progress} />
    </div>
  )
}

export function ButtonLoading() {
  return (
    <Loader2 className="h-4 w-4 animate-spin" aria-hidden="true" />
  )
}
