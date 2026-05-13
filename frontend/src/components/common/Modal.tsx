import React from 'react'
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogFooter,
} from '@/components/ui/dialog'
import { cn } from '@/lib/utils'
import { X } from 'lucide-react'
import { Button } from '@/components/ui/button'

interface ModalProps {
  open: boolean
  onClose: () => void
  title: string
  description?: string
  children: React.ReactNode
  size?: 'sm' | 'md' | 'lg' | 'xl' | 'full'
  closeOnBackdropClick?: boolean
  showCloseButton?: boolean
  footer?: React.ReactNode
  className?: string
}

const sizeClasses = {
  sm: 'max-w-sm',
  md: 'max-w-md',
  lg: 'max-w-lg',
  xl: 'max-w-xl',
  full: 'max-w-[95vw] h-[95vh]'
}

export function Modal({
  open,
  onClose,
  title,
  description,
  children,
  size = 'md',
  closeOnBackdropClick = true,
  showCloseButton = true,
  footer,
  className
}: ModalProps) {
  const handleOpenChange = (isOpen: boolean) => {
    if (!isOpen && closeOnBackdropClick) {
      onClose()
    }
  }

  return (
    <Dialog open={open} onOpenChange={handleOpenChange}>
      <DialogContent 
        className={cn(sizeClasses[size], className)}
        onEscapeKeyDown={onClose}
      >
        {showCloseButton && (
          <button
            onClick={onClose}
            className="absolute right-4 top-4 rounded-sm opacity-70 ring-offset-background transition-opacity hover:opacity-100 focus:outline-none focus:ring-2 focus:ring-ring focus:ring-offset-2 disabled:pointer-events-none"
            aria-label="Close"
          >
            <X className="h-4 w-4" />
            <span className="sr-only">Close</span>
          </button>
        )}
        
        <DialogHeader>
          <DialogTitle>{title}</DialogTitle>
          {description && (
            <DialogDescription>{description}</DialogDescription>
          )}
        </DialogHeader>
        
        <div className="py-4">
          {children}
        </div>
        
        {footer && (
          <DialogFooter>
            {footer}
          </DialogFooter>
        )}
      </DialogContent>
    </Dialog>
  )
}

// Confirmation Dialog variant
interface ConfirmDialogProps {
  open: boolean
  onClose: () => void
  onConfirm: () => void | Promise<void>
  title: string
  description: string
  variant?: 'info' | 'warning' | 'danger' | 'success'
  confirmText?: string
  cancelText?: string
  isLoading?: boolean
}

export function ConfirmDialog({
  open,
  onClose,
  onConfirm,
  title,
  description,
  variant = 'info',
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  isLoading = false
}: ConfirmDialogProps) {
  const [loading, setLoading] = React.useState(false)

  const handleConfirm = async () => {
    setLoading(true)
    try {
      await onConfirm()
      onClose()
    } catch (error) {
      console.error('Confirmation failed:', error)
    } finally {
      setLoading(false)
    }
  }

  return (
    <Modal
      open={open}
      onClose={onClose}
      title={title}
      description={description}
      size="sm"
      closeOnBackdropClick={!loading && !isLoading}
      footer={
        <div className="flex gap-2 justify-end w-full">
          <Button
            variant="outline"
            onClick={onClose}
            disabled={loading || isLoading}
          >
            {cancelText}
          </Button>
          <Button
            variant={variant === 'danger' ? 'destructive' : 'default'}
            onClick={handleConfirm}
            disabled={loading || isLoading}
          >
            {(loading || isLoading) ? 'Processing...' : confirmText}
          </Button>
        </div>
      }
    >
      {/* Empty children to satisfy Modal props */}
      <div />
    </Modal>
  )
}
