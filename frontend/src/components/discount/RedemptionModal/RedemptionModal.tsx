import { useState, useEffect, useRef } from 'react';
import { Button } from '@/components/ui/button';
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogFooter,
  DialogHeader,
  DialogTitle,
} from '@/components/ui/dialog';
import { useRedeemDiscount } from '../../../hooks/discount/useRedeemDiscount';
import type { DiscountType, RedemptionResponse } from '../../../types/discount.types';
import { toast } from 'sonner';
import { Check, Copy } from 'lucide-react';

interface RedemptionModalProps {
  discount: DiscountType | null;
  open: boolean;
  onOpenChange: (open: boolean) => void;
}

/**
 * Modal component for confirming and completing discount redemption
 */
export const RedemptionModal = ({ discount, open, onOpenChange }: RedemptionModalProps) => {
  const [redemptionResult, setRedemptionResult] = useState<RedemptionResponse | null>(null);
  const [copied, setCopied] = useState(false);
  const { mutate: redeemDiscount, isPending } = useRedeemDiscount();
  const confirmButtonRef = useRef<HTMLButtonElement>(null);
  const doneButtonRef = useRef<HTMLButtonElement>(null);

  // Focus management: Focus confirm/done button when modal opens or state changes
  useEffect(() => {
    if (open) {
      if (redemptionResult && doneButtonRef.current) {
        // Focus Done button after successful redemption
        setTimeout(() => doneButtonRef.current?.focus(), 100);
      } else if (confirmButtonRef.current) {
        // Focus Confirm button on initial open
        setTimeout(() => confirmButtonRef.current?.focus(), 100);
      }
    }
  }, [open, redemptionResult]);

  // Keyboard navigation: Enter to confirm, Escape handled by Dialog
  const handleKeyDown = (e: React.KeyboardEvent) => {
    if (e.key === 'Enter' && !isPending && !redemptionResult) {
      e.preventDefault();
      handleConfirmRedeem();
    }
  };

  const handleConfirmRedeem = () => {
    if (!discount) return;

    redeemDiscount(discount.id, {
      onSuccess: (data) => {
        setRedemptionResult(data);
      },
    });
  };

  const handleCopyCode = async () => {
    if (!redemptionResult) return;

    try {
      await navigator.clipboard.writeText(redemptionResult.code);
      setCopied(true);
      toast.success('Code copied to clipboard!');

      setTimeout(() => setCopied(false), 2000);
    } catch (err) {
      toast.error('Failed to copy code');
    }
  };

  const handleClose = () => {
    setRedemptionResult(null);
    setCopied(false);
    onOpenChange(false);
  };

  if (!discount) return null;

  return (
    <Dialog open={open} onOpenChange={handleClose}>
      <DialogContent 
        className="sm:max-w-md" 
        onKeyDown={handleKeyDown}
        aria-describedby={redemptionResult ? "redemption-success" : "redemption-confirm"}
      >
        {!redemptionResult ? (
          // Confirmation View
          <>
            <DialogHeader>
              <DialogTitle id="redemption-confirm">Confirm Redemption</DialogTitle>
              <DialogDescription>
                Are you sure you want to redeem this discount?
              </DialogDescription>
            </DialogHeader>

            <div className="space-y-4 py-4">
              <div 
                className="flex items-center justify-between p-4 bg-muted rounded-lg"
                role="region"
                aria-label="Discount details"
              >
                <div>
                  <p className="text-sm text-muted-foreground">Discount</p>
                  <p className="text-2xl font-bold" aria-label={`${discount.discountPercentage} percent off`}>{discount.discountPercentage}% OFF</p>
                </div>
                <div className="text-right">
                  <p className="text-sm text-muted-foreground">Cost</p>
                  <p className="text-xl font-semibold text-primary" aria-label={`${discount.requiredPoints.toLocaleString()} points`}>
                    {discount.requiredPoints.toLocaleString()} pts
                  </p>
                </div>
              </div>

              <p className="text-sm text-muted-foreground">
                This discount code can be used for your next subscription purchase.
                {discount.expiryDate && (
                  <> The code will expire on {new Date(discount.expiryDate).toLocaleDateString()}.</>
                )}
              </p>
            </div>

            <DialogFooter className="sm:justify-between">
              <Button
                type="button"
                variant="outline"
                onClick={handleClose}
                disabled={isPending}
                aria-label="Cancel redemption"
              >
                Cancel
              </Button>
              <Button
                ref={confirmButtonRef}
                type="button"
                onClick={handleConfirmRedeem}
                disabled={isPending}
                aria-label={isPending ? 'Redeeming discount code' : 'Confirm redemption'}
                aria-busy={isPending}
              >
                {isPending ? 'Redeeming...' : 'Confirm Redemption'}
              </Button>
            </DialogFooter>
          </>
        ) : (
          // Success View
          <>
            <DialogHeader>
              <DialogTitle id="redemption-success" className="flex items-center gap-2">
                <span className="text-2xl" role="img" aria-label="celebration">🎉</span>
                Redemption Successful!
              </DialogTitle>
              <DialogDescription>
                Your discount code has been issued. Copy it now!
              </DialogDescription>
            </DialogHeader>
            <div 
              role="status" 
              aria-live="polite" 
              className="sr-only"
            >
              Redemption successful. Your discount code is {redemptionResult.code}
            </div>

            <div className="space-y-4 py-4">
              <div 
                className="p-4 bg-green-50 border-2 border-green-200 rounded-lg"
                role="region"
                aria-label="Discount code details"
              >
                <p className="text-sm text-green-700 mb-2">Your Discount Code:</p>
                <div className="flex items-center justify-between">
                  <code 
                    className="text-2xl font-mono font-bold text-green-900"
                    aria-label={`Discount code: ${redemptionResult.code}`}
                  >
                    {redemptionResult.code}
                  </code>
                  <Button
                    size="icon"
                    variant="outline"
                    onClick={handleCopyCode}
                    className="shrink-0"
                    aria-label={copied ? 'Code copied to clipboard' : 'Copy discount code to clipboard'}
                  >
                    {copied ? <Check className="h-4 w-4" aria-hidden="true" /> : <Copy className="h-4 w-4" aria-hidden="true" />}
                  </Button>
                </div>
              </div>

              <div className="space-y-2 text-sm">
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Discount:</span>
                  <span className="font-semibold">{redemptionResult.discountPercentage}% OFF</span>
                </div>
                <div className="flex justify-between">
                  <span className="text-muted-foreground">Points Spent:</span>
                  <span className="font-semibold">{redemptionResult.pointsDeducted.toLocaleString()} pts</span>
                </div>
              </div>

              <p className="text-xs text-muted-foreground bg-amber-50 p-2 rounded">
                💡 {redemptionResult.message || 'You can view this code anytime in My Discounts page.'}
              </p>
            </div>

            <DialogFooter>
              <Button 
                ref={doneButtonRef}
                type="button" 
                onClick={handleClose} 
                className="w-full"
                aria-label="Close redemption modal"
              >
                Done
              </Button>
            </DialogFooter>
          </>
        )}
      </DialogContent>
    </Dialog>
  );
};
