import * as React from 'react';
import { AlertTriangle, Info, CheckCircle, XCircle } from 'lucide-react';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { Button } from '@/components/ui/button';
import { cn } from '@/lib/utils';

export type ConfirmDialogVariant = 'info' | 'warning' | 'danger' | 'success';

export interface ConfirmDialogProps {
  open: boolean;
  onOpenChange: (open: boolean) => void;
  title: string;
  description: string;
  confirmText?: string;
  cancelText?: string;
  variant?: ConfirmDialogVariant;
  onConfirm: () => void | Promise<void>;
  onCancel?: () => void;
  isLoading?: boolean;
}

const variantConfig: Record<
  ConfirmDialogVariant,
  {
    icon: React.ElementType;
    iconClassName: string;
    confirmButtonVariant: 'default' | 'destructive';
  }
> = {
  info: {
    icon: Info,
    iconClassName: 'text-blue-500',
    confirmButtonVariant: 'default',
  },
  warning: {
    icon: AlertTriangle,
    iconClassName: 'text-yellow-500',
    confirmButtonVariant: 'default',
  },
  danger: {
    icon: XCircle,
    iconClassName: 'text-red-500',
    confirmButtonVariant: 'destructive',
  },
  success: {
    icon: CheckCircle,
    iconClassName: 'text-green-500',
    confirmButtonVariant: 'default',
  },
};

export const ConfirmDialog: React.FC<ConfirmDialogProps> = ({
  open,
  onOpenChange,
  title,
  description,
  confirmText = 'Confirm',
  cancelText = 'Cancel',
  variant = 'info',
  onConfirm,
  onCancel,
  isLoading = false,
}) => {
  const [isProcessing, setIsProcessing] = React.useState(false);
  const config = variantConfig[variant];
  const Icon = config.icon;

  const handleConfirm = async () => {
    try {
      setIsProcessing(true);
      await onConfirm();
      onOpenChange(false);
    } catch (error) {
      console.error('Error in confirm action:', error);
      // Error handling can be extended here
    } finally {
      setIsProcessing(false);
    }
  };

  const handleCancel = () => {
    onCancel?.();
    onOpenChange(false);
  };

  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !isProcessing && !isLoading) {
      e.preventDefault();
      handleConfirm();
    }
  };

  return (
    <Dialog open={open} onOpenChange={onOpenChange}>
      <DialogContent
        className="sm:max-w-[425px]"
        onKeyDown={handleKeyDown}
      >
        <DialogHeader>
          <div className="flex items-center gap-3">
            <div
              className={cn(
                'flex h-10 w-10 items-center justify-center rounded-full',
                variant === 'info' && 'bg-blue-100 dark:bg-blue-900/30',
                variant === 'warning' && 'bg-yellow-100 dark:bg-yellow-900/30',
                variant === 'danger' && 'bg-red-100 dark:bg-red-900/30',
                variant === 'success' && 'bg-green-100 dark:bg-green-900/30'
              )}
            >
              <Icon className={cn('h-5 w-5', config.iconClassName)} />
            </div>
            <DialogTitle className="text-left">{title}</DialogTitle>
          </div>
          <DialogDescription className="text-left pt-2">
            {description}
          </DialogDescription>
        </DialogHeader>
        <DialogFooter className="flex-row gap-2 sm:justify-end">
          <Button
            type="button"
            variant="outline"
            onClick={handleCancel}
            disabled={isProcessing || isLoading}
          >
            {cancelText}
          </Button>
          <Button
            type="button"
            variant={config.confirmButtonVariant}
            onClick={handleConfirm}
            disabled={isProcessing || isLoading}
            className="min-w-[100px]"
          >
            {isProcessing || isLoading ? (
              <span className="flex items-center gap-2">
                <span className="h-4 w-4 animate-spin rounded-full border-2 border-current border-t-transparent" />
                Processing...
              </span>
            ) : (
              confirmText
            )}
          </Button>
        </DialogFooter>
      </DialogContent>
    </Dialog>
  );
};
