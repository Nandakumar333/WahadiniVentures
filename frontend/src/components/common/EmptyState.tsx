import type { LucideIcon } from 'lucide-react'
import { Package, Search, AlertCircle, CheckCircle2 } from 'lucide-react'
import { Button } from '@/components/ui/button'
import { cn } from '@/lib/utils'

interface EmptyStateProps {
  icon?: LucideIcon
  title: string
  description: string
  action?: {
    label: string
    onClick: () => void
  }
  illustration?: string // URL to image
  variant?: 'no-data' | 'no-results' | 'error' | 'success'
  className?: string
}

const defaultIcons = {
  'no-data': Package,
  'no-results': Search,
  'error': AlertCircle,
  'success': CheckCircle2
}

const variantStyles = {
  'no-data': 'text-muted-foreground',
  'no-results': 'text-blue-500',
  'error': 'text-destructive',
  'success': 'text-green-500'
}

export function EmptyState({
  icon,
  title,
  description,
  action,
  illustration,
  variant = 'no-data',
  className
}: EmptyStateProps) {
  const Icon = icon || defaultIcons[variant]
  const iconColor = variantStyles[variant]

  return (
    <div className={cn('flex flex-col items-center justify-center p-8 text-center', className)}>
      {illustration ? (
        <img 
          src={illustration} 
          alt={title}
          className="w-64 h-64 object-contain mb-6 opacity-50"
        />
      ) : (
        <div className={cn('mb-6 p-6 rounded-full bg-muted/50', iconColor)}>
          <Icon className="w-12 h-12" aria-hidden="true" />
        </div>
      )}
      
      <h3 className="text-xl font-semibold mb-2">
        {title}
      </h3>
      
      <p className="text-muted-foreground max-w-md mb-6">
        {description}
      </p>
      
      {action && (
        <Button onClick={action.onClick} variant="default">
          {action.label}
        </Button>
      )}
    </div>
  )
}

// Specialized variants for common use cases
export function NoDataState({ 
  title = 'No data yet', 
  description = 'Get started by creating your first item.',
  action
}: Partial<EmptyStateProps>) {
  return (
    <EmptyState
      variant="no-data"
      title={title}
      description={description}
      action={action}
    />
  )
}

export function NoSearchResultsState({
  searchQuery,
  description = 'Try adjusting your search or filters to find what you\'re looking for.',
}: {
  searchQuery?: string
  description?: string
}) {
  return (
    <EmptyState
      variant="no-results"
      title={searchQuery ? `No results for "${searchQuery}"` : 'No results found'}
      description={description}
    />
  )
}

export function ErrorState({
  title = 'Something went wrong',
  description = 'We encountered an error loading this content. Please try again.',
  action
}: Partial<EmptyStateProps>) {
  return (
    <EmptyState
      variant="error"
      title={title}
      description={description}
      action={action}
    />
  )
}

export function SuccessState({
  title = 'All done!',
  description = 'Everything is complete.',
}: Partial<EmptyStateProps>) {
  return (
    <EmptyState
      variant="success"
      title={title}
      description={description}
    />
  )
}
